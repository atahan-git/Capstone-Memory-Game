using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class RandomClipPlayer : MonoBehaviour {

    [FormerlySerializedAs("clips")] public AudioClip[] attackClips;

    public AudioClip[] deathClips;
    
    public AudioClip[] getHitClips;

    private AudioSource _source;

    private void Awake() {
        _source = GetComponent<AudioSource>();
    }

    public void PlayAttackClip() {
        if (attackClips.Length > 0) {
            _source.Stop();
            _source.clip = attackClips[Random.Range(0, attackClips.Length)];
            _source.Play();
        }
    }

    public void PlayDeathClip() {
        if (deathClips.Length > 0) {
            _source.clip = deathClips[Random.Range(0, deathClips.Length)];
            _source.Play();
        }
    }

    public void PlayGetHitClip() {
        if (getHitClips.Length > 0) {
            _source.clip = getHitClips[Random.Range(0, getHitClips.Length)];
            _source.Play();
        }
    }
}
