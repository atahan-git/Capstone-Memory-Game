using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;


public class PlayerLoadoutController : MonoBehaviour {
	public static PlayerLoadoutController s;

	public Color[] elementColors = new[] { Color.magenta, Color.red, Color.blue, };

	public static readonly Dictionary<string, int> elementNameToId = new Dictionary<string, int>() {
		{ "arcane", 0 },
		{ "fire", 1},
		{ "ice", 2}
	};

	public GameObject[] spells = new GameObject[3];
	private WordPack[] selectedWordPacks;

	private void Awake() {
		if (s == null) {
			s = this;
		}
	}

	private void Start() {
		if (s == this) {
			RefreshLoadout();
		}
	}

	private void OnDestroy() {
		DataSaver.loadEvent -= RefreshLoadout;
	}


	public void RefreshLoadout() {
		var mySave = DataSaver.s.GetCurrentSave();

		if (mySave.loadoutWordPackNames == null || mySave.loadoutWordPackNames.Length < 3 || mySave.loadoutWordPackNames[0] == null) {
			SetDefaultLoadoutWordPacks(mySave);
		}

		selectedWordPacks = new WordPack[3];

		for (int i = 0; i < mySave.loadoutWordPackNames.Length; i++) {
			selectedWordPacks[i] = WordPackLoader.s.allWordPacks.Find(pack => pack.wordPackName == mySave.loadoutWordPackNames[i]);
		}
	}

	public static void SetDefaultLoadoutWordPacks(DataSaver.SaveFile mySave) {
		try {
			mySave.loadoutWordPackNames = new[] {
				WordPackLoader.s.allWordPacks.Find(pack => pack.wordPackAffinity == "arcane").wordPackName,
				WordPackLoader.s.allWordPacks.Find(pack => pack.wordPackAffinity == "fire").wordPackName,
				WordPackLoader.s.allWordPacks.Find(pack => pack.wordPackAffinity == "ice").wordPackName,
			};
			
			DataSaver.s.SaveActiveGame();
		} catch {
			WordPackLoader.s.LoadDefaultWordPacksFromResources();
		}
	}


	public WordPack GetWordPackWithIndex(int index) {
		if (selectedWordPacks == null || selectedWordPacks.Length < 3) {
			RefreshLoadout();
		}
		return selectedWordPacks[index];
	}
	
	public GameObject GetSpellWithIndex(int index) {
		return spells[index];
	}
}
