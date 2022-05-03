using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu_SwitchController : MonoBehaviour {
    public GameObject[] menus = new GameObject[5];
    public GameObject[] buttons = new GameObject[5];

    public int defaultMenu = 2;
    
    public Color buttonSelectedColor = Color.gray;
    public Color buttonRegularColor = Color.white;
    public Color textSelectedColor = Color.white;
    public Color textRegularColor = Color.black;

    private void Start() {
        Init_MultiMenuSwitch(menus, buttons, defaultMenu);
    }


    public void Init_MultiMenuSwitch(GameObject[] menus, GameObject[] buttons, int defaultMenu) {
        
        var positions = new Vector3[menus.Length];
        for (int i = 0; i < menus.Length; i++) {
            positions[i] = menus[i].GetComponent<RectTransform>().anchoredPosition3D;
            menus[i].SetActive(true);
        }
        
        for (int i = 0; i < buttons.Length; i++) {
            int m = i;
            buttons[i].GetComponentInChildren<Button>().onClick.AddListener(() => MultiMenuSwitch(menus, buttons, positions, m));
        }

        MultiMenuSwitch(menus, buttons, positions, defaultMenu);
    }
    
    public void MultiMenuSwitch(GameObject[] menus, GameObject[] buttons, Vector3[] basePos, int index) {
        for (int i = 0; i < menus.Length; i++) {
            if (i != index) {
                menus[i].GetComponent<RectTransform>().anchoredPosition3D = new Vector3(10000, 0, 0);
            } else {
                menus[i].GetComponent<RectTransform>().anchoredPosition3D = basePos[i];
            }
        }
        
        for (int i = 0; i < buttons.Length; i++) {
            if (i != index) {
                var myButton = buttons[i];
                myButton.transform.Find("Button").GetComponent<Image>().color = buttonRegularColor;
                myButton.transform.Find("Button").Find("Text").GetComponent<TMP_Text>().color = textRegularColor;
                myButton.transform.Find("Button").Find("Icon").GetComponent<Image>().color = textRegularColor;
            } else {
                var myButton = buttons[i];
                myButton.transform.Find("Button").GetComponent<Image>().color = buttonSelectedColor;
                myButton.transform.Find("Button").Find("Text").GetComponent<TMP_Text>().color = textSelectedColor;
                myButton.transform.Find("Button").Find("Icon").GetComponent<Image>().color = textSelectedColor;
            }
        }
    } 
}
