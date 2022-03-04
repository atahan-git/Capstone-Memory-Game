using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleEffectController : MonoBehaviour{
	private ParticleSystem[] parts;

	public GameObject toActivate;

	private void Awake() {
		parts = GetComponentsInChildren<ParticleSystem>();
	}

	public void ActivateEffect() {
		toActivate.SetActive(true);
		for (int i = 0; i < parts.Length; i++) {
			parts[i].Play();
		}
	}


	public void DisableEffect() {
		toActivate.SetActive(false);
		for (int i = 0; i < parts.Length; i++) {
			parts[i].Stop();
		}
	}
}
