using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public GameObject[] spells = new GameObject[3];
    public Transform spellParent;

    public Vector2 shootInterval = new Vector2(2, 4);
    public Vector2Int shootCount = new Vector2Int(7, 10);
    public Vector2 reloadInterval = new Vector2(5, 10);

    private void Start() {
        StartCoroutine(ShootCoroutine());
    }


    IEnumerator ShootCoroutine() {
        var timer = 0f;
        while (true) {
            var spell = spells[Random.Range(0, spells.Length)];
            bool isAOE = Random.Range(0, 2) == 0;

            int curShootCount = Random.Range(this.shootCount.x, this.shootCount.y);

            timer = Random.Range(reloadInterval.x, reloadInterval.y);
            while (timer > 0) {
                timer -= Time.deltaTime;

                // pause
                while (!MapController.s.isPlaying) {
                    yield return null;
                }
                
                yield return null;
            }

            for (int i = 0; i < curShootCount; i++) {
                var curActiveSpell = Instantiate(spell, spellParent).GetComponent<ISpell>();

                timer = Random.Range(shootInterval.x, shootInterval.y);
                while (timer > 0) {
                    timer -= Time.deltaTime;
                    
                    // pause
                    while (!MapController.s.isPlaying) {
                        yield return null;
                    }
                    
                    yield return null;
                }

                curActiveSpell.Engage(MapController.s.myLanes[Random.Range(0, MapController.s.myLanes.Length)], true, isAOE);
            }
        }
    }
}
