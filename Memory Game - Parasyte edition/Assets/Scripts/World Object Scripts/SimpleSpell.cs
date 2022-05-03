using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpell : MonoBehaviour, ISpell, IGameStateUpdateDependent {

    public GameObject casting;
    public GameObject projectile;

    public Monster target;

    public Crystal myCrystal;

    private void Start() {
        casting.SetActive(true);
        projectile.SetActive(false);
        MapController.s.myDependents.Add(this);
    }

    public bool isEngaged = false;
    public bool isComingFromLeft = false;
    public bool isAOE = false;
    public Lane myLane;

    public void Engage(Lane lane, bool _isComingFromLeft, bool _isAOE) {
        if (!isEngaged) {
            casting.SmartDestroy();
            projectile.SetActive(true);
            isComingFromLeft = _isComingFromLeft;
            isAOE = _isAOE;
            myLane = lane;
            
            foreach (var monster in MapController.s.myLanes[0].activeMonsters) {
                if (monster.isStructure && monster.isComingFromLeft == isComingFromLeft) {
                    var crystal = monster.GetComponent<Crystal>();
                    if (crystal != null) {
                        myCrystal = crystal;
                        break;
                    }
                }
            }
            

            target = FindNearestEnemy();
            isEngaged = true;
        }
    }

    public void DestroySelf() {
        gameObject.SmartDestroy();
    }

    public float damage = 4;
    public float aoeDamage = 2;
    public float explosionRange = 2;

    public float speed = 0.5f;
    public float acceleration = 0.01f;
    
    public void UpdateSelf() {
        if (isEngaged) {
            if (target != null) {
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
                speed += acceleration * Time.deltaTime;

                if (Vector3.Distance(transform.position, target.transform.position) < 0.1f) {
                    Explode();
                }
            } else {
                gameObject.SmartDestroy();
            }
        }
    }

    public GameObject destroyEffect;
    public void Explode() {
        if (isAOE) {
            var enemiesInRange = FindMonstersInRange(explosionRange);
            for (int i = 0; i < enemiesInRange.Count; i++) {
                enemiesInRange[i].Damage(aoeDamage);
            }
        } else {
            target.Damage(damage);
        }
        
        Instantiate(destroyEffect, transform.position, transform.rotation);

        gameObject.SmartDestroy();
    }
    
    Monster FindNearestEnemy() {
        Monster nearestEnemy = null;
        float distance = float.MaxValue;

        for (int i = 0; i < myLane.activeMonsters.Count; i++) {
            var curMonster = myLane.activeMonsters[i];

            if (isComingFromLeft != curMonster.isComingFromLeft) {
                var curDistance = Vector3.Distance(curMonster.transform.position, myCrystal.transform.position) - curMonster.width;
                if (curDistance < distance) {
                    distance = curDistance;
                    nearestEnemy = curMonster;
                }
            }
        }


        return nearestEnemy;
    }
    
    List<Monster> FindMonstersInRange(float range) {
        List<Monster> enemiesInRange = new List<Monster>();

        for (int i = 0; i < myLane.activeMonsters.Count; i++) {
            var curMonster = myLane.activeMonsters[i];

            if (isComingFromLeft != curMonster.isComingFromLeft) {
                var curDistance = Vector3.Distance(curMonster.transform.position, transform.position) - curMonster.width;
                if (curDistance < range) {
                    enemiesInRange.Add(curMonster);
                }
            }
        }


        return enemiesInRange;
    }


    private void OnDestroy() {
        if(MapController.s != null)
            MapController.s.myDependents.Remove(this);
    }
}
