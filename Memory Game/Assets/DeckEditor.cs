using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditor : MonoBehaviour {
    private MainMenu_DeckMenuController master;
    public WordPack myPack;

    public bool canEdit = false;

    public GameObject wordPairPrefab;
    public GameObject addNewWordPairPrefab;
    public GameObject activeAddNewWord;
    public Transform wordsParent;

    public GameObject areYouSureDialog;
    public TMP_Text dialogText;
    
    public GameObject editButton;
    public GameObject goBackButton;
    public GameObject saveButton;
    public GameObject discardButton;
    
    public enum DialogStates {
        save, discard, delete
    }

    private DialogStates curDialogState;


    public bool isWordPackDirty = false;

    public void Initialize(WordPack pack, MainMenu_DeckMenuController _master) {
        myPack = pack;
        master = _master;
        GetComponentInChildren<MiniGUI_DeckInfo>().Initialize(pack, master);

        isWordPackDirty = false;
        
        DrawDetailedWords();
    }

    void SetEditState(bool state) {
        canEdit = state;

        for (int i = 0; i < wordsParent.childCount; i++) {
            wordsParent.GetChild(i).GetComponent<MiniGUI_WordPairWithEdit>()?.UpdateEditableStatus(canEdit);
        }

        if (canEdit) {
            if (activeAddNewWord == null) {
                activeAddNewWord = Instantiate(addNewWordPairPrefab, wordsParent);
                activeAddNewWord.GetComponentInChildren<Button>().onClick.AddListener(AddWordPair);
            }
        }

        editButton.SetActive(!canEdit);
        goBackButton.SetActive(!canEdit);
        saveButton.SetActive(canEdit);
        discardButton.SetActive(canEdit);
    }
    
    void DrawDetailedWords() {
        wordsParent.DeleteAllChildren();
        
        var packProgress = DataSaver.s.GetCurrentSave().GetProgress(myPack);
        for (int i = 0; i < myPack.wordPairs.Count; i++) {
            var progress = packProgress.GetWordPairData(myPack.wordPairs[i]);

            var display = Instantiate(wordPairPrefab, wordsParent).GetComponent<MiniGUI_WordPairWithEdit>();
            display.Initialize(myPack.wordPairs[i], progress, this);
        }

        SetEditState(canEdit);
    }


    public void SaveChanges() {
        curDialogState = DialogStates.save;
        dialogText.text = "Are you sure you want to SAVE your changes?";
        areYouSureDialog.SetActive(true);
    }
    
    public void DiscardChanges() {
        curDialogState = DialogStates.discard;
        dialogText.text = "Are you sure you want to DISCARD your changes?";
        areYouSureDialog.SetActive(true);
    }

    private MiniGUI_WordPairWithEdit toDeleteWordPair;
    public void DeleteDialog(MiniGUI_WordPairWithEdit wordPair) {
        curDialogState = DialogStates.delete;
        dialogText.text = $"Are you sure you want to delete the word pair {wordPair.wordPair.meaning} - {wordPair.wordPair.word}?";
        areYouSureDialog.SetActive(true);

        toDeleteWordPair = wordPair;
    }

    public void DialogResult(bool result) {
        if (result) {
            switch (curDialogState) {
                case DialogStates.save:
                    WordPackLoader.SaveWordPack(myPack);
                    master.WordPacksWereChanged();
                    DrawDetailedWords();

                    SetEditState(false);
                    break;
                case DialogStates.discard:
                    var index = WordPackLoader.s.allWordPacks.IndexOf(myPack);
                    WordPackLoader.s.allWordPacks.Remove(myPack);
                    WordPackLoader.s.allWordPacks.Insert(index, WordPackLoader.LoadWordPack(myPack.wordPackName));
                    master.WordPacksWereChanged();
                    DrawDetailedWords();

                    SetEditState(false);
                    break;
                case DialogStates.delete:
                    myPack.wordPairs.Remove(toDeleteWordPair.wordPair);
                    Destroy(toDeleteWordPair.gameObject);
                    break;
            }
        }
        areYouSureDialog.SetActive(false);
    }


    public void Edit() {
        SetEditState(true);
    }

    public void GoBack() {
        if (canEdit && isWordPackDirty) {
            DiscardChanges();
        } else {
            master.SetInspectOverlayState(false);
        }
    }

    public void WordsChanged() {
        isWordPackDirty = true;
    }

    public void AddWordPair() {
        myPack.wordPairs.Add(new WordPair(){id = myPack.wordPairs[myPack.wordPairs.Count-1].id + 1});
        DrawDetailedWords();
    }
}
