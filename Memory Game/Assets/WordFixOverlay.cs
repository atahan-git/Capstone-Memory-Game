using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordFixOverlay : MonoBehaviour {

    public TMP_InputField word;
    public TMP_InputField meaning;

    private WordPair myPair;

    private void Start() {
        Hide();
    }

    public void Engage(WordPair toChange) {
        myPair = toChange;
        word.text = myPair.word;
        meaning.text = myPair.meaning;
        gameObject.SetActive(true);
    }


    public void Save() {
        myPair.word = word.text;
        myPair.meaning = meaning.text;
        
        Hide();
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
