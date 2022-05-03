using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordSystemController : MonoBehaviour {
	public string[] wordPackNames = new string[3];
		
	public Animator animWordDisplay;

	public TMP_Text statusText;
	public TMP_Text wordText;
	public TMP_Text meaningText;
	
	public Slider timeSlider;
	public Image sliderFill;
	
	public Color regularColor = Color.cyan;
	public Color newColor = Color.yellow;
	public Color wrongColor = Color.red;
	public Color correctColor = Color.green;

	public WordPack activeWordPack;
	public DataSaver.UserWordPackProgress activeUserProgress;
	public bool activeSide;
	public int currentWordIndex;
	public int activeWordPackIndex;
	public int curWordCount = 0;
	public bool isAOE = false;

	public float wordPickTimer = 30f;
	public float newWordTimer = 10f;
	public float wrongWordTimer = 10f;
	public float correctWordTimer = 1.6f;
	public int wordCountPerPack = 10;

	public WordSystemSoundController soundController;

	public KeySupressor keySupressor;

	private void Start() {
		MobileKeyboardCapturer.s.StartListening(TryMatchWord);
		MobileKeyboardCapturer.s.valueChangedCallback += OnValueChanged;
	}

	private void Update() {
		if (Input.GetMouseButtonDown(1)) {
			StopWordMatchMode();
		}
	}

	public void OnValueChanged(string word) {
		soundController.MakeTypingSound();
	}

	public void ActivateWordPack(int buttonid) {
		Debug.Log($"Activating word pack for button: {buttonid}");
		//return;
		StopWordMatchMode();
		activeSide = buttonid >= 3;
		activeWordPackIndex = buttonid - (activeSide ? 3 : 0);
		isAOE = activeSide;
		

		var packName = wordPackNames[activeWordPackIndex];
		activeWordPack = WordPackLoader.s.allWordPacks.Find((pack => pack.wordPackName == packName));
		
		var index = DataSaver.s.GetCurrentSave().wordPackData.FindIndex((progress => progress.wordPackName == packName));
		if (index != -1) {
			activeUserProgress = DataSaver.s.GetCurrentSave().wordPackData[index];
		} else {
			activeUserProgress = new DataSaver.UserWordPackProgress();
			activeUserProgress.wordPackName = packName;
			DataSaver.s.GetCurrentSave().wordPackData.Add(activeUserProgress);
		}
		
		curWordCount = 0;
		ActivateWordMatchMode();
	}

	public void ActivateWordMatchMode() {
		SwitchWord(true);
		animWordDisplay.SetBool("isWord", true);
	}

	public void StopWordMatchMode() {
		ClearState();
		animWordDisplay.ResetAllAnimatorTriggers();
		animWordDisplay.SetBool("isWord", false);
		//animWordDisplay.SetTrigger("reset");
		
		if(isNewWordPaused)
			PauseController.s.Resume();
	}

	public void BeginTimedSection() {
		if(timerToStart != null)
			activeTimer = StartCoroutine(timerToStart);
	}

	public float wordChangeDelay = 0.2f;
	public void SwitchWord(bool isFirstTime = false) {
		if(!isFirstTime)
			animWordDisplay.SetTrigger("refreshWord");
		
		curWordCount++;

		if (curWordCount >= wordCountPerPack) {
			StopWordMatchMode();
			return;
		}
		
		currentWordIndex = Scheduler.GetNextWordPairIndex(activeWordPack, activeUserProgress, activeSide);

		var currentWord = activeWordPack.wordPairs[currentWordIndex];
		StartCoroutine(DelayedChangeWord(currentWord));
		

		var userWordPairProgress = activeUserProgress.GetWordPairData(currentWordIndex);
		
		
		if (userWordPairProgress.type == 0) {
			NewWord();
		} else {
			StandardWordMatch();
		}
	}

	IEnumerator DelayedChangeWord(WordPair currentWord) {
		yield return new WaitForSeconds(wordChangeDelay);
		wordText.text = activeSide ? currentWord.word : currentWord.meaning;
		meaningText.text = !activeSide ? currentWord.word : currentWord.meaning;
	}
	
	private InputChecker _inputChecker;
	private Coroutine activeTimer;
	private IEnumerator timerToStart;

	void ClearState() {
		_inputChecker = null;
		
		if(activeTimer != null)
			StopCoroutine(activeTimer);
		
		timerToStart = null;
	}

	public bool isNewWordPaused = false;
	public void NewWord() {
		ClearState();
		sliderFill.color = newColor;
		_inputChecker = NewWordInputChecker;
		statusText.text = "New Word";
		
		animWordDisplay.SetTrigger("newWord");
		
		PauseController.s.Pause();
		isNewWordPaused = true;
		timeSlider.value = 1;
		
		soundController.NewWordSoundEffect();
	}


	public void StandardWordMatch() {
		ClearState();
		sliderFill.color = regularColor;
		_inputChecker = StandardInputChecker;
		statusText.text = "";
		
		animWordDisplay.SetTrigger("guess");

		timerToStart = Timer(wordPickTimer, OutOfTime);
		BeginTimedSection();
		
		
		soundController.StandardMatchSoundEffect();
	}
	
	

	public void CorrectMatch() {
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, currentWordIndex, activeSide, true);
		
		ClearState();
		sliderFill.color = correctColor;
		_inputChecker = TransitionInputChecker;
		statusText.text = "Correct!";
		
		animWordDisplay.SetTrigger("correct");

		keySupressor.AddCharges();
		
		timerToStart = Timer(correctWordTimer, () => SwitchWord());
		BeginTimedSection();

		soundController.CorrectMatchSoundEffect();
	}
	
	public void OutOfTime() {
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, currentWordIndex, activeSide, false);
		
		ClearState();
		sliderFill.color = wrongColor;
		_inputChecker = TransitionInputChecker;
		statusText.text = "Out of time!";
		
		animWordDisplay.SetTrigger("wrong");
		
		timerToStart = Timer(wrongWordTimer, () => SwitchWord());
		BeginTimedSection();
		
		
		soundController.OutOfTimeSoundEffect();
	}

	public void WrongMatch() {
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, currentWordIndex, activeSide, false);
		
		ClearState();
		sliderFill.color = wrongColor;
		_inputChecker = TransitionInputChecker;
		statusText.text = "Wrong Match";
		
		animWordDisplay.SetTrigger("wrong");
		
		timerToStart = Timer(wrongWordTimer, () => SwitchWord());
		BeginTimedSection();
		
		soundController.WrongMatchSoundEffect();
	}
	
	delegate void Callback();
	delegate void InputChecker(string inputChecker);

	IEnumerator Timer(float time, Callback callback) {
		var timer = time;

		while (timer > 0) {
			timeSlider.value = timer / time;
			timer -= Time.deltaTime;
			yield return null;
		}

		callback?.Invoke();
	}

	public SimpleEffectController[] ActiveLaneShower = new SimpleEffectController[2];

	public bool IsEmpty(string word) {
		return word.Replace(" ", "") == "";
	}

	public bool IsMatch(string word1, string word2) {
		return StringDistance.LevenshteinDistance(word1, word2) <= 1;
	}

	public void TryMatchWord(string word) {
		if (_inputChecker != null) {
			_inputChecker(word);
		}

		if (word.Length > 0) {
			soundController.MakeEnterSound();
		}
	}


	public void NewWordInputChecker(string word) {
		if (IsMatch(word, meaningText.text)) {
			Scheduler.RegisterResult(activeWordPack, activeUserProgress, currentWordIndex, activeSide, true);
			PauseController.s.Resume();
			isNewWordPaused = false;
			SwitchWord();
		} else {
			animWordDisplay.SetTrigger("wrong");
		}
	}
	
	public void StandardInputChecker(string word) {
		if (!IsEmpty(word)) {
			if (IsMatch(word, meaningText.text)) {
				CorrectMatch();
			} else {
				WrongMatch();
			}
		}
	}

	public void TransitionInputChecker(string word) {
		SwitchWord();
	}

}

public interface ISpell {
	void Engage(Lane lane, bool isComingFromLeft, bool isAOE);
	void DestroySelf();
}