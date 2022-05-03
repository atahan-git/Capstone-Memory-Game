using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class MonsterSpawnButton : MonoBehaviour
{
    public GameObject myMonsterToSpawn;
    public GameObject myIconMonster;

    public Vector3 iconMonsterDefPos;
    public Camera mainCam;

    public RectTransform topLaneRect;
    public RectTransform bottomLaneRect;

    private int monsterCost;

    public TMP_Text costText;

    private void Start() {
        iconMonsterDefPos = myIconMonster.transform.localPosition;
        mainCam = Camera.main;
        monsterCost = myMonsterToSpawn.GetComponent<Monster>().energyCost;
        costText.text = monsterCost.ToString();

        SetIconMonster();
    }

    public int iconMonsterSortingOrder = 10;

    void SetIconMonster() {
        myIconMonster.transform.DeleteAllChildren();
        var iconMonster = Instantiate(myMonsterToSpawn, myIconMonster.transform);
        iconMonster.transform.localPosition = Vector3.zero;
        iconMonster.transform.localScale = Vector3.one;

        iconMonster.GetComponentInChildren<MeshRenderer>().sortingOrder = iconMonsterSortingOrder;
        
        var comps = iconMonster.GetComponents<MonoBehaviour>();

        for (int i = 0; i < comps.Length; i++) {
            Destroy(comps[i]);
        }
    }

    public void OnButtonDown () {
        isEngaged = true;
    }


    public bool isEngaged = false;
    private void Update() {
        if (isEngaged) {
            if (Input.touchCount > 0 || Input.GetMouseButton(0)) {
                var pos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                myIconMonster.transform.position = new Vector3(pos.x,pos.y, myIconMonster.transform.position.z);
            } else {

                if (EnergyController.s.playerEnergy >= monsterCost) {
                    if (MouseInRect(topLaneRect)) {
                        MapController.s.SpawnMonster(myMonsterToSpawn, 0, false, 1);
                        EnergyController.s.RemoveEnergy(monsterCost, true);
                        
                    } else if (MouseInRect(bottomLaneRect)) {
                        MapController.s.SpawnMonster(myMonsterToSpawn, 1, false, 1);
                        EnergyController.s.RemoveEnergy(monsterCost, true);
                        
                    }
                }

                isEngaged = false;
            }
        } else {
            myIconMonster.transform.localPosition = iconMonsterDefPos;
        }
    }


    bool MouseInRect(RectTransform rect) {
        if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, mainCam)) {
            return true;
        } else {
            return false;
        }
    }
}
