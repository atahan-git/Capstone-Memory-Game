using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordPackStats {
	public readonly int learnedWordCount = 0;
	public readonly int masteredWordCount = 0;
	public readonly int needPracticeCount = 0;
	public readonly int notSeenCount = 0;
	public readonly int totalWordCount = 0;
	public readonly Color elementalColor;
	
	public WordPackStats(WordPack pack) {
		
		DataSaver.UserWordPackProgress progress = DataSaver.s.GetCurrentSave().GetProgress(pack);
		totalWordCount = pack.wordPairs.Count;

		if (progress != null) {
			for (int i = 0; i < pack.wordPairs.Count; i++) {
				var pairProgress = progress.GetWordPairData(pack.wordPairs[i]);
				if (pairProgress.type == 0) {
					notSeenCount += 1;
				} else {
					learnedWordCount += 1;

					if (Scheduler.needPractice(pairProgress)) {
						needPracticeCount += 1;
					}
					
					if (Scheduler.isMastered(pairProgress)) {
						masteredWordCount += 1;
					}
				}
			}
		} else {
			notSeenCount = pack.wordPairs.Count;
		}

		try {

			elementalColor = PlayerLoadoutController.s.elementColors[PlayerLoadoutController.elementNameToId[pack.wordPackAffinity]];
		} catch {
			Debug.LogError($"Unknown Word Pack Affinity -{pack.wordPackAffinity}- in word pack -{pack.wordPackName}- ");
		}
	}
	public string GetStats() {
		return  $"Learned: {learnedWordCount}\n" +
		        $"Mastered: {masteredWordCount}\n" +
		        $"Need Practice: {needPracticeCount}\n" +
		        $"Not Seen: {notSeenCount}\n";
	}

	public string GetDetailedPower() {
		var practicePer = ((float)needPracticeCount / totalWordCount * 100f).ToString("F0");
		var notSeenPer = (((float)notSeenCount / totalWordCount) * 0.25f * 100f).ToString("F0");
		var masteryPer = (((float)masteredWordCount / totalWordCount)*0.75f * 100f).ToString("F0");
		var total = (GetPower() * 100f).ToString("F0");
		return $"Base 100% + Words Needing Practice {practicePer}% + Not Seen  {notSeenPer}% - Mastery {masteryPer}% = {total}%";
	}
	
	public float GetPower() {
		var power = 1f;

		power += (float)needPracticeCount / totalWordCount;
		
		power += ((float)notSeenCount / totalWordCount) * 0.25f;

		power -=  ((float)masteredWordCount / totalWordCount)*0.75f;
		
		return power;
	}
}
