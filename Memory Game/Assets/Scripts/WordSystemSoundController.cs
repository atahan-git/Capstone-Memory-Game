using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WordSystemSoundController : MonoBehaviour {

    [SerializeField]  AudioClip[] typingSounds;
    [SerializeField] private AudioClip enterSound;

    [SerializeField] AudioClip correctSound;
    [SerializeField]  AudioClip wrongSound;
    [SerializeField]  AudioClip outOfTimeSound;
    [SerializeField]  AudioClip newWordSound;
    [SerializeField]  AudioClip standardSound;

    [SerializeField]  AudioSource typingSource;
    [SerializeField]  AudioSource wordMatchSounds;


    public void MakeEnterSound() {
        typingSource.clip = enterSound;
        typingSource.pitch = Random.Range(0.9f, 1.1f);
        typingSource.Play();
    }
    
    public void MakeTypingSound() {
        typingSource.clip = typingSounds[Random.Range(0, typingSounds.Length)];
        typingSource.pitch = Random.Range(0.9f, 1.1f);
        typingSource.Play();
    }


    public void CorrectMatchSoundEffect() {
        /*wordMatchSounds.clip = correctSound;
        wordMatchSounds.Play();*/
    }


    public void WrongMatchSoundEffect() {
        wordMatchSounds.clip = wrongSound;
        wordMatchSounds.Play();
    }

    public void OutOfTimeSoundEffect() {
        wordMatchSounds.clip = outOfTimeSound;
        wordMatchSounds.Play();
    }

    public void NewWordSoundEffect() {
        wordMatchSounds.clip = newWordSound;
        wordMatchSounds.Play();
    }

    public void StandardMatchSoundEffect() {
        wordMatchSounds.clip = standardSound;
        wordMatchSounds.Play();
    }
}
