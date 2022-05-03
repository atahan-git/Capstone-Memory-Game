using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnergyBarDisplayAnimations : MonoBehaviour {

	public TMP_Text amountText;
	public Transform toScale;
	public float curEnergy = 0;
	public float lerpSpeed = 0.3f;

	public GameObject particles;
	public bool particleStatus = true;

	public Vector2 scalingAmountRange = new Vector2(50, 1000);
	public Vector2 scalingTransformRange = new Vector2(0.8f, 1.3f);

	public int particleStartAmount = 250;


	private void Update() {
		var energy = EnergyController.s.playerEnergy;
		
		curEnergy = Mathf.Lerp(curEnergy, energy, lerpSpeed*Time.deltaTime);
		
		var mappedEnergy = curEnergy.Remap(scalingAmountRange.x, scalingAmountRange.y, scalingTransformRange.x, scalingTransformRange.y);
		mappedEnergy = Mathf.Clamp(mappedEnergy, scalingTransformRange.x, scalingTransformRange.y);

		toScale.transform.localScale = Vector3.one * mappedEnergy;

		amountText.text = Mathf.RoundToInt(curEnergy).ToString();
		
		

		if (energy > particleStartAmount) {
			if (!particleStatus) {
				particleStatus = true;
				particles.SetParticleSystems(true);
			}
		} else {
			if (particleStatus) {
				particleStatus = false;
				particles.SetParticleSystems(false);
			}
		}
	}
}
