using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpell : MonoBehaviour, ISpell, IGameStateUpdateDependent {

    public GameObject casting;
    public GameObject projectile;

    public Monster target;

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
            SmartDestroy(casting);
            projectile.SetActive(true);
            isComingFromLeft = _isComingFromLeft;
            isAOE = _isAOE;
            myLane = lane;

            target = FindNearestEnemy();
            isEngaged = true;
        }
    }

    public void DestroySelf() {
        SmartDestroy(gameObject);
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
                SmartDestroy(gameObject);
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

        SmartDestroy(gameObject);
    }
    
    Monster FindNearestEnemy() {
        Monster nearestEnemy = null;
        float distance = 10;

        for (int i = 0; i < myLane.activeMonsters.Count; i++) {
            var curMonster = myLane.activeMonsters[i];

            if (isComingFromLeft != curMonster.isComingFromLeft) {
                var curDistance = Vector3.Distance(curMonster.transform.position, transform.position) - curMonster.width;
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

    void SmartDestroy(GameObject target) {
        if (target != null) {
            var particles = target.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < particles.Length; i++) {
                particles[i].Stop();
                Destroy(particles[i].gameObject, 3f);
                particles[i].transform.SetParent(null);
            }

            Destroy(target);
        }
    }


    private void OnDestroy() {
        if(MapController.s != null)
            MapController.s.myDependents.Remove(this);
    }
}
