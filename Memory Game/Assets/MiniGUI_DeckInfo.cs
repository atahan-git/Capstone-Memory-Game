using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_DeckInfo : MonoBehaviour {

	public Image icon;
	
	public TMP_Text deckName;
	public TMP_Text deckStats;
	public TMP_Text deckWords;
	
	public Slider deckPowerSlider;
	public TMP_Text deckDetailedPowerText;
	public TMP_Text deckPowerText;

	public int wordDisplayCount = 6;

	private MainMenu_DeckMenuController master;
	public WordPack myPack;

	public Button equipButton;
	public TMP_Text equipButtonText;
	public void Initialize(WordPack pack, MainMenu_DeckMenuController _master) {
		master = _master;
		myPack = pack;
		
		var stats = new WordPackStats(pack);
		
		deckName.text = pack.wordPackName;
		if (deckStats != null) {
			deckStats.text = stats.GetStats();
		}

		if (deckWords != null) {
			DrawLimitedWords();
		}


		var power = stats.GetPower();

		deckPowerSlider.value = power;
		
		if(deckPowerText != null)
			deckPowerText.text = (power * 100).ToString("F0") + "%";

		if (deckDetailedPowerText != null)
			deckDetailedPowerText.text = stats.GetDetailedPower();

		icon.color = stats.elementalColor;
	}

	public void DisableEquipButton() {
		equipButton.interactable = false;
		equipButtonText.text = "Already Equipped";
	}

	void DrawLimitedWords() {
		if (myPack.wordPairs.Count <= wordDisplayCount) {
			deckWords.text = GetWordsFromWordPack(myPack, 0, wordDisplayCount);
		} else {
			deckWords.text = GetWordsFromWordPack(myPack, 0, wordDisplayCount - 1);
			deckWords.text += $"\nand {myPack.wordPairs.Count - wordDisplayCount} more";
		}
	}

	

	public void InspectWordPack() {
		master.InspectWordPack(myPack);
	}

	public void ChangeLoadoutWordPack() {
		master.ChangeLoadoutWordPack(myPack);
	}


	// End is not inclusive
	string GetWordsFromWordPack(WordPack pack, int start, int end) {
		var words = "";

		for (int i = start; i < end; i++) {
			if (i >= pack.wordPairs.Count) {
				break;
			} else {
				words += pack.wordPairs[i].word + " - " + pack.wordPairs[i].meaning + "\n";
			}
		}

		if (words.Length > 2) {
			return words.Substring(0, words.Length-2);
		} else {
			return words;
		}
	}
}
