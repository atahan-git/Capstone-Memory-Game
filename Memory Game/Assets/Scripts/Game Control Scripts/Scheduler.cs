using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Scheduler {
	
	/* Algorithm inspired by:
	 * https://learning.mpi-sws.org/memorize/
	 * https://github.com/ankitects/anki
	 * https://github.com/slaypni/SM-15
	 */
	
	/* We want to have our scheduler follow some guidelines and be random:
	 *	- The more "forgotten" a word is the more likely it can be shown
	 *  - When all of the words are mostly remembered there should be a chance of learning a new word.
	 *  - The math should work out that the more words someone knows the more review time and the less new words being learned
	 *  - There should be a way to dial difficulty.
	 *			- This dial make it so that new words are more likely to show and review words are less likely
	 *			- Which should result in words being reviewed later in their forgetting cycle
	 *	- The amount of new words shouldn't change their chance of appearing.
	 */
	
	/*	Each new word gets a number from 0 - 100, where 0 is 100% remembrance and 100 is 50% remembrance
	 *  New word gets number 50, so around the same value as 75% remembrance.
	 *	Then we collect all these numbers and run a weighted randomization.
	 */
	
	
	static int lastMatch = -1; // make sure we don't show the same word twice

	static float RecallProbability(float forgettingRate, DateTime currentTime, DateTime lastReviewTime) {
		return Mathf.Pow(2, -forgettingRate * (float)(currentTime - lastReviewTime).TotalMinutes);
	}


	private static float newWordWeight = 50;
	
	private static float rightRate = -0.5f;
	private static float wrongRate = 1;
	private static float defRate = 0.5f;
	private static float backSideRate = 0.3f;
	static float ForgettingRate(int nCorrect, int nWrong, int backSideNCorrect, int backSideNWrong) {
		float correct = nCorrect + (backSideNCorrect * backSideRate);
		float wrong = nWrong + (backSideNWrong * backSideRate);
		return defRate * Mathf.Pow(2, (rightRate * correct + wrongRate * wrong));
	}

	static float ProbabilityToWeight(float probability) {
		return (1 - Mathf.Max((probability - 0.5f) * 2, 0)) * 100;
	}

	static float[] GenerateWeights(DataSaver.UserWordPackProgress progress, bool isMeaningSide, bool includeNewWord) {
		var wordPairData = progress.GetWordPairData();

		var weightCount = wordPairData.Count;
		if (includeNewWord)
			weightCount += 1;

		float[] weights = new float[weightCount];

		DateTime currentTime = DateTime.Now;

		for (int i = 0; i < wordPairData.Count; i++) {
			var data = wordPairData[i];
			if (data.type != 0) {
				var forgetRate = ForgettingRate(
					data.GetCorrect(isMeaningSide), data.GetWrong(isMeaningSide), 
					data.GetCorrect(!isMeaningSide), data.GetWrong(!isMeaningSide)
					);
				var lastRecallTime = DateTime.FromFileTimeUtc(data.GetLastRecallUtcFileTime(isMeaningSide));
				var recallProb = RecallProbability(forgetRate, currentTime, lastRecallTime);
				weights[i] = ProbabilityToWeight(recallProb);
			}/* else {
				weights[i] = 0;
			}*/
		}

		if(includeNewWord)
			weights[weights.Length-1] = newWordWeight;

		return weights;
	}

	

	// We want the user to feel like they mastered a word if they recalled it 20 times correctly, and recalled it more correctly than wrong
	public static bool isMastered(DataSaver.UserWordPairProgress progress) {
		var correctCount = Mathf.Min(progress.GetCorrect(true), progress.GetCorrect(false));
		var wrongCount = Mathf.Max(progress.GetWrong(true), progress.GetWrong(false));

		return (correctCount > 20) && (correctCount > wrongCount * 3);
	}

	
	// We want the word to need practice if the recall change of either side is less than 70%
	public static bool needPractice(DataSaver.UserWordPairProgress progress) {
		DateTime currentTime = DateTime.Now;
		
		var forgetRate1 =  ForgettingRate(
			progress.GetCorrect(true), progress.GetWrong(true), 
			progress.GetCorrect(false), progress.GetWrong(false)
		);
		var recallTime1 = DateTime.FromFileTimeUtc(progress.GetLastRecallUtcFileTime(true));
		var recallProb1 = RecallProbability(forgetRate1, currentTime, recallTime1);
		
			
		var forgetRate2 =  ForgettingRate(
			progress.GetCorrect(false), progress.GetWrong(false), 
			progress.GetCorrect(true), progress.GetWrong(true)
		);
		var recallTime2 = DateTime.FromFileTimeUtc(progress.GetLastRecallUtcFileTime(false));
		var recallProb2 = RecallProbability(forgetRate2, currentTime, recallTime2);

		var leastRecallProb = Mathf.Min(recallProb1, recallProb2);

		return leastRecallProb < 0.7f;
	}

	static int GetReviewWord(DataSaver.UserWordPackProgress progress, bool isMeaningSide, bool includeNewWord) {
		var weights = GenerateWeights(progress, isMeaningSide, includeNewWord);

		int nextMatch = GetRandomWeightedIndex(weights);

		while (nextMatch == lastMatch) {
			nextMatch = GetRandomWeightedIndex(weights);
		}

		if (includeNewWord) {
			if (nextMatch == weights.Length - 1) {
				nextMatch = -1;
			}
		}

		lastMatch = nextMatch; // to make sure we dont get the same word twice in a row
		return nextMatch;
	}

	static int GetNewWord(WordPack wordData, DataSaver.UserWordPackProgress progress, bool isMeaningSide) {
		for (int i = 0; i < wordData.wordPairs.Count; i++) {
			if (progress.GetWordPairData(wordData.wordPairs[i]).type == 0) {
				return i;
			}
		}

		return -1;
	}
	
	public static WordPair GetNextWordPair(WordPack wordData, DataSaver.UserWordPackProgress progress, bool isMeaningSide) {
		var nextMatch = GetReviewWord(progress, isMeaningSide, true);
		
		if (nextMatch == -1) { // If we get -1, it means we picked new word! so get a new word
			nextMatch = GetNewWord(wordData, progress, isMeaningSide);
		}

		if (nextMatch == -1) { // if it is still -1, it means we have run out of new words in this pack, so lets pick another word instead.
			nextMatch = GetReviewWord(progress, isMeaningSide, false);
		}

		return wordData.wordPairs[nextMatch];
	}
	
	
	public static void RegisterResult(WordPack wordData, DataSaver.UserWordPackProgress progress, WordPair wordPair, bool isMeaningSide, bool isCorrect) {
		progress.GetWordPairData(wordPair).type = 1;
		progress.GetWordPairData(wordPair).Increment(isMeaningSide, isCorrect);
		
		DataSaver.s.SaveActiveGame();
	}
	
	
	
	public static int GetRandomWeightedIndex(float[] weights)
	{
		if(weights == null || weights.Length == 0) return -1;
 
		float total = 0;
		int i;
		for(i = 0; i < weights.Length; i++)
		{
			total += weights[i];
		}
 
		float r = Random.value;
		float s = 0f;
 
		for(i = 0; i < weights.Length; i++)
		{
     
			s += weights[i] / total;
			if (s >= r) return i;
		}
 
		return -1;
	}
}


public static class StringDistance
{
	public static string RemoveSpecialCharacters(string str)
	{
		return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
	}
	
	/// <summary>
	/// Compute the distance between two strings.
	/// </summary>
	public static int LevenshteinDistance(string s, string t) {
		s = RemoveSpecialCharacters(s);
		t = RemoveSpecialCharacters(t);
		s = s.ToLowerInvariant();
		t = t.ToLowerInvariant();
		
		int n = s.Length;
		int m = t.Length;
		int[,] d = new int[n + 1, m + 1];

		// Step 1
		if (n == 0)
		{
			return m;
		}

		if (m == 0)
		{
			return n;
		}

		// Step 2
		for (int i = 0; i <= n; d[i, 0] = i++)
		{
		}

		for (int j = 0; j <= m; d[0, j] = j++)
		{
		}

		// Step 3
		for (int i = 1; i <= n; i++)
		{
			//Step 4
			for (int j = 1; j <= m; j++)
			{
				// Step 5
				int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

				// Step 6
				d[i, j] = Mathf.Min(
					Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
					d[i - 1, j - 1] + cost);
			}
		}
		// Step 7
		return d[n, m];
	}
}