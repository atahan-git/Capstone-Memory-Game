using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu_DeckMenuController : MonoBehaviour {
    public MainMenu_SwitchController switchController;

    public GameObject[] menus;
    public GameObject[] buttons;


    public MiniGUI_DeckInfo[] loadoutPackDisplays;
    public int currentlySelected = 0;
    
    
    public GameObject otherDecksPrefab;
    public Transform otherDecksParent;


    public GameObject inspectOverlay;
    private Vector3 inspectOverlayLocation;

    void Start() {
        switchController.Init_MultiMenuSwitch(menus, buttons, 0);
        DrawLoadoutPacks();
        DrawOtherDecks();

        inspectOverlayLocation = inspectOverlay.GetComponent<RectTransform>().anchoredPosition3D;
        inspectOverlay.SetActive(true);
        SetInspectOverlayState(false);
    }

    public void SetInspectOverlayState(bool isOpen) {
        if (isOpen) {
            inspectOverlay.GetComponent<RectTransform>().anchoredPosition3D = inspectOverlayLocation;
        } else {
            inspectOverlay.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(10000, 0, 0);
        }
    }

    void DrawLoadoutPacks() {
        for (int i = 0; i < loadoutPackDisplays.Length; i++) {
            loadoutPackDisplays[i].Initialize(PlayerLoadoutController.s.GetWordPackWithIndex(i), this);
        }
    }


    public void ChangeCurrentlySelected(int changeTo) {
        currentlySelected = changeTo;
        DrawOtherDecks();
    }

    void DrawOtherDecks() {
        otherDecksParent.DeleteAllChildren();

        var allWordPacks = WordPackLoader.s.allWordPacks;
        for (int i = 0; i < allWordPacks.Count; i++) {
            var affinity = PlayerLoadoutController.elementNameToId[allWordPacks[i].wordPackAffinity];
            if (affinity == currentlySelected) {
                var deckInfo = Instantiate(otherDecksPrefab, otherDecksParent);
                deckInfo.GetComponent<MiniGUI_DeckInfo>().Initialize(allWordPacks[i], this);

                if (allWordPacks[i].wordPackName == loadoutPackDisplays[currentlySelected].myPack.wordPackName) {
                    deckInfo.GetComponent<MiniGUI_DeckInfo>().DisableEquipButton();
                }
            }
        }
    }

    public void ChangeLoadoutWordPack(WordPack changeTo) {
        DataSaver.s.GetCurrentSave().loadoutWordPackNames[currentlySelected] = changeTo.wordPackName;
        PlayerLoadoutController.s.RefreshLoadout();
        DrawLoadoutPacks();
        DrawOtherDecks();
    }
    
    public void InspectWordPack(WordPack changeTo) {
        inspectOverlay.GetComponent<DeckEditor>().Initialize(changeTo,this);
        if (changeTo.wordPackName == loadoutPackDisplays[currentlySelected].myPack.wordPackName) {
            inspectOverlay.GetComponentInChildren<MiniGUI_DeckInfo>().DisableEquipButton();
        }
        
        SetInspectOverlayState(true);
    }


    public void WordPacksWereChanged() {
        DrawOtherDecks();
    }


    public void ReloadDefaultWordPacks() {
        WordPackLoader.s.LoadDefaultWordPacksFromResources();
        DrawOtherDecks();
    }
}
