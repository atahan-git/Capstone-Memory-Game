using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour {

    public static PauseController s;

    
    public GameObject pausedStatusShow;
    public GameObject manualPauseScreen;


    public delegate void pauseCallback();

    public pauseCallback GamePausedCall;
    public pauseCallback GameResumeCall;
    
    
    private void Awake() {
        s = this;
    }

    private void Start() {
        Resume();
        pausedStatusShow.SetActive(false);
        manualPauseScreen.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ManualPause();
        }
    }

    public bool isPlaying = true;

    public void ManualPause() {
        if (isPlaying) {
            Pause();
            pausedStatusShow.SetActive(false);
            manualPauseScreen.SetActive(true);
        }
    }

    public void Pause() {
        if (isPlaying) {
            isPlaying = false;
            pausedStatusShow.SetActive(true);
            GamePausedCall?.Invoke();
        }
    }

    public void Resume() {
        if (!isPlaying) {
            isPlaying = true;
            pausedStatusShow.SetActive(false);
            manualPauseScreen.SetActive(false);
            GameResumeCall?.Invoke();
            MobileKeyboardCapturer.s.ReFocus();
        }
    }
}
