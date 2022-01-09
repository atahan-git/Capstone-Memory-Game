using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleEffectController : MonoBehaviour{
	private ParticleSystem[] parts;

	private void Awake() {
		parts = GetComponentsInChildren<ParticleSystem>();
	}

	public void ActivateEffect() {
		for (int i = 0; i < parts.Length; i++) {
			parts[i].Play();
		}
	}


	public void DisableEffect() {
		for (int i = 0; i < parts.Length; i++) {
			parts[i].Stop();
		}
	}
}
