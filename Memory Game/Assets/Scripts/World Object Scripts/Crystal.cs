using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Crystal : MonoBehaviour {

    public Lane otherLane;
    
    public Slider hpSlider;
    public TMP_Text hpText;

    private Monster _monster;

    private void Start() {
        _monster = GetComponent<Monster>();
        otherLane.activeMonsters.Add(_monster);
    }


    // Update is called once per frame
    void Update() {
        hpSlider.value = _monster.healthPercent;

        hpText.text = _monster.healthPercent.ToString("P0");

        if (_monster.health <= 0) {
            SceneLoader.s.LoadMenuScene();
        }
    }
}
