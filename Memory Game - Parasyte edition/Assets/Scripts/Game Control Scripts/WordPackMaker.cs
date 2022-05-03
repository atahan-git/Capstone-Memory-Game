using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

public class WordPackMaker : MonoBehaviour {

    [ReadOnly]
    public List<string> allWordPacks = new List<string>();
    
    public WordPack activeWordPack;

    [Button()]
    public void SaveWordPack() {
        WordPackLoader.SaveWordPack(activeWordPack);
        RefreshAllWordPacks();
    }
    
    
    [Button()]
    public void LoadWordPack() {
        activeWordPack = WordPackLoader.LoadWordPack(activeWordPack.wordPackName);
        RefreshAllWordPacks();
    }


    [Button()]
    public void RefreshAllWordPacks() {
        var paths = WordPackLoader.GetAllWordPackPaths();
        allWordPacks = new List<string>();
        
        for (int i = 0; i < paths.Length; i++) {
            allWordPacks.Add(Path.GetFileNameWithoutExtension(paths[i]));
        }
    }
}
