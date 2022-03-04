using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MobileKeyboardCapturer : MonoBehaviour {
    public static MobileKeyboardCapturer s;

    private void Awake() {
        s = this;
    }

    public TMP_InputField inputField;

    public TMP_Text status;

    private void Start() {
        Application.targetFrameRate = 60;
        
        TouchScreenKeyboard.Open("yeet", TouchScreenKeyboardType.Default, false,false, false, false, "yeet");
        //TouchScreenKeyboard.Android.closeKeyboardOnOutsideTap = false;

        
        inputField.onDeselect.AddListener(OnDeselect);
        inputField.onSubmit.AddListener(OnEditEnd);
        inputField.onEndEdit.AddListener(OnDeselect);
        
        
        inputField.ActivateInputField();
    }
    

    public void OnDeselect(string word) {
        inputField.ActivateInputField();
    }

    public void OnEditEnd(string word) {
        if (isListening) {
            _callback?.Invoke(inputField.text);
            /*isListening = false;
            _callback = null;*/
        }
        
        inputField.text = "";
        inputField.ActivateInputField();
    }


    public delegate void wordCapturedCallback(string word);

    private wordCapturedCallback _callback;
    private bool isListening = false;
    public void StartListening(wordCapturedCallback callback) {
        inputField.text = "";
        _callback = callback;
        isListening = true;
    }

    public void StopListening() {
        isListening = false;
        _callback = null;
    }
}

