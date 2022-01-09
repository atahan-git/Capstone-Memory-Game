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
        while (true) {
            var spell = spells[Random.Range(0, spells.Length)];
            bool isAOE = Random.Range(0, 2) == 0;

            int curShootCount = Random.Range(this.shootCount.x, this.shootCount.y);

            yield return new WaitForSeconds(Random.Range(reloadInterval.x, reloadInterval.y));

            for (int i = 0; i < curShootCount; i++) {
                var curActiveSpell = Instantiate(spell, spellParent).GetComponent<ISpell>();

                yield return new WaitForSeconds(Random.Range(shootInterval.x, shootInterval.y));

                curActiveSpell.Engage(MapController.s.myLanes[Random.Range(0, MapController.s.myLanes.Length)], true, isAOE);
            }
        }
    }
    
    
}
