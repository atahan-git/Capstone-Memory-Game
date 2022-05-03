using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class WordPack {
	public string wordPackName = "unset";
	public string wordPackAffinity = "unset";

	public List<WordPair> wordPairs = new List<WordPair>();
}


[Serializable]
public class WordPair {
	public int id = -1;
	public string word = ""; 
	public string meaning = "";
}