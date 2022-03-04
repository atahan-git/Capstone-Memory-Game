using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {
    public static MapController s;

    public List<IGameStateUpdateDependent> myDependents = new List<IGameStateUpdateDependent>();

    private void Awake() {
        s = this;
    }

    private void Start() {
        Unpause();
    }

    public Lane[] myLanes = new Lane[2];

    public void SpawnMonster(GameObject prefab, int lane, bool isComingFromLeft, int monsterLevel) {

        float position = isComingFromLeft ? 0 : 1;

        var curLane = myLanes[lane];

        var monster = Instantiate(prefab, curLane.GetPositionBetweenWalls(position), Quaternion.identity).GetComponent<Monster>();
        monster.isComingFromLeft = isComingFromLeft;
        monster.myLane = curLane;
        monster.SetLevel(monsterLevel);
        if (!isComingFromLeft) {
            monster.transform.localScale = new Vector3(-1, 1, 1);
        }
    }


    public GameObject pausedStatusShow;
    
    public bool isPlaying = true;

    public void Pause() {
        isPlaying = false;
        pausedStatusShow.SetActive(true);
        
        for (int i = 0; i < myLanes.Length; i++) {
            for (int j = 0; j < myLanes[i].activeMonsters.Count; j++) {
                myLanes[i].activeMonsters[j].PauseMonster();
            }
        }
    }

    public void Unpause() {
        isPlaying = true;
        pausedStatusShow.SetActive(false);
        
        for (int i = 0; i < myLanes.Length; i++) {
            for (int j = 0; j < myLanes[i].activeMonsters.Count; j++) {
                myLanes[i].activeMonsters[j].UnpauseMonster();
            }
        }
    }

    void Update() {
        if (isPlaying) {
            for (int i = 0; i < myLanes.Length; i++) {
                for (int j = 0; j < myLanes[i].activeMonsters.Count; j++) {
                    myLanes[i].activeMonsters[j].UpdateMonster();
                }
            }

            for (int i = 0; i < myDependents.Count; i++) {
                myDependents[i].UpdateSelf();
            }
        }
    }
}

public interface IGameStateUpdateDependent {
    void UpdateSelf();
}

