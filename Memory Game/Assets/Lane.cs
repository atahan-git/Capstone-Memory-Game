using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour{
	public Transform[] checkpoints = new Transform[4];

	public List<Monster> activeMonsters = new List<Monster>();

	
	private float deltaZMultiplier = 0.5f;
	private float defZ = -3;
	
	public Vector3 GetGfxPos(Vector3 pos) {
		var midY = checkpoints[1].transform.position.y;
		return new Vector3(0, 0, (pos.y - midY) * deltaZMultiplier + defZ);
	}
	
	public Vector3 GetPositionBetweenWalls(float percentage) {
		return Vector3.Lerp(checkpoints[1].position, checkpoints[2].position, percentage);
	}
}
