using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyController : MonoBehaviour {
    public static EnergyController s;

    public int playerEnergy = 0;
    public int enemyEnergy = 0;

    public GameObject energyFlyEffect;

    public Transform playerEnergyTarget;
    public Transform enemyEnergyTarget;

    private void Awake() {
        s = this;
    }

    public void AwardEnergy(int amount, Vector3 location, bool isComingFromLeft) {
        if (isComingFromLeft) {
            playerEnergy += amount;
        } else {
            enemyEnergy += amount;
        }
    }

    public void RemoveEnergy(int amount, bool isPlayer) {
        if (isPlayer) {
            playerEnergy -= amount;
        } else {
            enemyEnergy -= amount;
        }
    }
}
