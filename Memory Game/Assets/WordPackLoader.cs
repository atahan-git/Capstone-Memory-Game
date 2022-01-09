using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class WordPackLoader : MonoBehaviour {
    public static WordPackLoader s;

    public const string wordPackFolderName = "Word Packs";
    
    public static string GetWordPackPath () {
#if UNITY_ANDROID
        return Path.Combine( Application.dataPath , wordPackFolderName);
#else
        return Application.persistentDataPath;
#endif
        
    }


    public static string[] GetAllWordPackPaths() {
        return Directory.GetFiles(GetWordPackPath(), "*.json");
    }


    private void Awake() {
        if (s == null) {
            s = this;
            LoadWordPacks();
        }

        //MoveScriptablesToJson();

#if UNITY_ANDROID
        StartCoroutine(GetText("https://drive.google.com/uc?export=download&id=1ss-cad0sG5WepIvPIF5J1GFPdpZMpf0H", "Japanese Foods"));
        StartCoroutine(GetText("https://drive.google.com/uc?export=download&id=1BER4fKjxOne7eym-neJ32OvRSMZf3nEO", "Japanese Places"));
        StartCoroutine(GetText("https://drive.google.com/uc?export=download&id=1CQ2AbtUBaqgr6WWCtDoUZs3bgExEWQE4", "Japanese School"));
#endif
    }


    public List<WordPack> allWordPacks = new List<WordPack>();
    void LoadWordPacks() {
        var allPaths = GetAllWordPackPaths();
        allWordPacks = new List<WordPack>();

        
        Debug.Log($"Loading {allPaths.Length} word packs from {GetWordPackPath()}");

        for (int i = 0; i < allPaths.Length; i++) {
            var wordPack = DataSaver.ReadFile<WordPack>(allPaths[i]);
            
            allWordPacks.Add(wordPack);
        }

        loadedWordPacksText.text = allWordPacks.Count.ToString();
    }

    public TMP_Text loadedWordPacksText;

    IEnumerator GetText(string url, string wordPackName)
    {
        
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.Send();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else {
                var path = Path.Combine(GetWordPackPath(), wordPackName + ".json");
                System.IO.File.WriteAllText(path, www.downloadHandler.text);
                Debug.Log("Data Gathered Online " + wordPackName + " saved to " + path);
                LoadWordPacks();
            }
        }
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
