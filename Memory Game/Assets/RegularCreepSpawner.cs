using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularCreepSpawner : MonoBehaviour {

    public MapController cont;

    public GameObject regularCreep;

    public float startDelay = 2f;
    public float delay = 20f;

    private void Start() {
        InvokeRepeating("SpawnCreeps",startDelay, delay);
    }

    void SpawnCreeps() {
        cont.SpawnMonster(regularCreep, 0, false, 1);
        cont.SpawnMonster(regularCreep, 0, true, 2);
        cont.SpawnMonster(regularCreep, 1, false, 1);
        cont.SpawnMonster(regularCreep, 1, true, 2);
    }
}
