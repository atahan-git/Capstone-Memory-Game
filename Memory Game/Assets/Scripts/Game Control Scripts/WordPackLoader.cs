using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class WordPackLoader : MonoBehaviour {
    public static WordPackLoader s;

    public const string wordPackFolderName = "WordPacks";

    public static string GetWordPackPath () {
        return Path.Combine(Application.persistentDataPath, wordPackFolderName);
    }


    public static string[] GetAllWordPackPaths() {
        Directory.CreateDirectory(GetWordPackPath());
        return Directory.GetFiles(GetWordPackPath(), "*.json");
    }


    private void Awake() {
        if (s == null) {
            s = this;
            
            
            // add some checks here so that in the first load we load word packs but on the later ones we dont
            // so that the player can edit the word packs and change them/add more
            LoadDefaultWordPacksFromResources();
        }
    }


    public List<WordPack> allWordPacks = new List<WordPack>();
    void LoadWordPacks() {
        var allPaths = GetAllWordPackPaths();
        allWordPacks = new List<WordPack>();

        
        Debug.Log($"Loading {allPaths.Length} word packs from {GetWordPackPath()}");

        for (int i = 0; i < allPaths.Length; i++) {
            var wordPack = DataSaver.ReadFile<WordPack>(allPaths[i]);
            
            Debug.Log( $"{wordPack.wordPairs.Count} words found in word pack {wordPack.wordPackName}" );
            
            allWordPacks.Add(wordPack);
        }

        loadedWordPacksText.text = allWordPacks.Count.ToString();
    }

    public TMP_Text loadedWordPacksText;

    void LoadDefaultWordPacksFromResources() {
        
        print("--- START LOADING WORDPACKS FROM RESOURCES");
        var wordPacks = Resources.LoadAll<TextAsset>(wordPackFolderName);

        for (int i = 0; i < wordPacks.Length; i++) {
            var path = Path.Combine(GetWordPackPath(), wordPacks[i].name + ".json");
            File.WriteAllText(path, wordPacks[i].text);
            Debug.Log($"Word Pack -{wordPacks[i].name}- written to {path}");
        }

        print($"--- END LOADING WORD PACKS FROM RESOURCES total: {wordPacks.Length}");

        LoadWordPacks();
    }

    public static WordPack LoadWordPack(string wordPackName) {
        var path = Path.Combine(GetWordPackPath(), wordPackName + ".json");
        Debug.Log($"Load Word Pack:\"{wordPackName}\" from \"{path}\"");
        var wordPack = DataSaver.ReadFile<WordPack>(path);

        return wordPack;
    }

    public static void SaveWordPack(WordPack wordPack) {
        var path = Path.Combine(GetWordPackPath(), wordPack.wordPackName + ".json");
        Debug.Log($"Saving Word Pack:\"{wordPack.wordPackName}\" to \"{path}\"");
        DataSaver.WriteFile(path, wordPack);
    }
}
