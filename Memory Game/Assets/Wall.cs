using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Wall : MonoBehaviour {

    public Slider hpSlider;
    public TMP_Text hpText;

    private Monster _monster;

    private void Start() {
        _monster = GetComponent<Monster>();
    }


    // Update is called once per frame
    void Update() {
        hpSlider.value = _monster.healthPercent;

        hpText.text = _monster.healthPercent.ToString("P0");
    }
}
