using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularCreepSpawner : MonoBehaviour , IGameStateUpdateDependent{

    public MapController cont;

    public GameObject regularCreep;

    public float startDelay = 2f;
    public float delay = 20f;
    public float timer;

    private void Start() {
        MapController.s.myDependents.Add(this);
        timer = startDelay;
        //InvokeRepeating("SpawnCreeps",startDelay, delay);
    }
    
    

    void SpawnCreeps() {
        cont.SpawnMonster(regularCreep, 0, false, 1);
        cont.SpawnMonster(regularCreep, 0, true, 2);
        cont.SpawnMonster(regularCreep, 1, false, 1);
        cont.SpawnMonster(regularCreep, 1, true, 2);
    }

    public void UpdateSelf() {
        if (timer > 0) {
            timer -= Time.deltaTime;
        } else {
            timer = delay;
            SpawnCreeps();
        }
    }
}
