using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_WordPairWithEdit : MonoBehaviour {
    
    public Toggle isSeenToggle;
    public Toggle needPracticeToggle;
    public Toggle masteredToggle;

    public TMP_InputField wordField;
    public TMP_InputField meaningField;

    public GameObject deleteButton;

    private bool canEdit = false;
    public WordPair wordPair;
    private DataSaver.UserWordPairProgress progress;
    private DeckEditor master;
    
    public void Initialize(WordPair _wordPair, DataSaver.UserWordPairProgress _progress, DeckEditor _master) {
        wordPair = _wordPair;
        progress = _progress;
        master = _master;

        bool isSeen = _progress.type != 0;
        if (isSeen) {
            isSeenToggle.isOn = true;
            needPracticeToggle.isOn = Scheduler.needPractice(progress);
            masteredToggle.isOn = Scheduler.isMastered(progress);
        } else {
            isSeenToggle.isOn = false;
            needPracticeToggle.isOn = false;
            masteredToggle.isOn = false;
        }

        wordField.text = wordPair.word;
        meaningField.text = wordPair.meaning;
    }

    public void UpdateEditableStatus(bool _canEdit) {
        canEdit = _canEdit;

        wordField.interactable = canEdit;
        meaningField.interactable = canEdit;
        
        deleteButton.SetActive(canEdit);
    }


    public void WordPairChanged() {
        if(wordField.text.Length > 0)
            wordPair.word = wordField.text;
        if(meaningField.text.Length > 0)
            wordPair.meaning = meaningField.text;
        master.WordsChanged();
    }

    public void DeleteWord() {
        master.DeleteDialog(this);
    }
}
