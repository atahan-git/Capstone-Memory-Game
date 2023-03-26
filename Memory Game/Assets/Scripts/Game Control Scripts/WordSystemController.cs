using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordSystemController : MonoBehaviour {
	public Transform spellParent;

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
	public WordPair activeWordPair;
	public Lane activeLane;
	public ISpell activeSpell;
	public int curWordCount = 0;
	public bool isAOE = false;
	public GameObject activeSpellPrefab;

	public float wordPickTimer = 60f;
	//public float newWordTimer = 10f;
	public float wrongWordTimer = 10f;
	public float correctWordTimer = 2f;
	public float wordChangeDelay = 0.2f;
	public int wordCountPerPack = 10;

	public WordSystemSoundController soundController;
	
	public float slowDownSpeed = 0.01f;

	public Slider manaSlider;
	public float mana = 1f;
	public float manaFillSpeed = 0.05f;
	public float spellCastManaUse = 0.33f;

	public GameObject notEnoughManaOverlay;
	public GameObject alreadyCastingSpellOverlay;

	private void Start() {
		ChangeShootLane(0);
		
		MobileKeyboardCapturer.s.StartListening(TryMatchWord);
		MobileKeyboardCapturer.s.valueChangedCallback += OnValueChanged;
		StopWordMatchMode();
	}

	private void Update() {
		if (Input.GetMouseButtonDown(1)) {
			StopWordMatchMode();
		}

		mana += manaFillSpeed * Time.deltaTime;
		mana = Mathf.Clamp01(mana);
		manaSlider.value = mana;

		if (alreadyCastingSpellOverlay.activeSelf) {
			notEnoughManaOverlay.SetActive(false);
		} else {
			notEnoughManaOverlay.SetActive(mana < spellCastManaUse);
		}
	}

	public void OnValueChanged(string word) {
		soundController.MakeTypingSound();
	}

	public void ActivateWordPack(int buttonid) {
		Debug.Log($"Activating word pack for button: {buttonid}");
		
		if(mana < spellCastManaUse)
			return;

		mana -= spellCastManaUse;
		
		//return;
		StopWordMatchMode();
		activeSide = buttonid >= 3;
		var toActivateIndex = buttonid - (activeSide ? 3 : 0);
		isAOE = activeSide;

		activeWordPack = PlayerLoadoutController.s.GetWordPackWithIndex(toActivateIndex);

		activeUserProgress = DataSaver.s.GetCurrentSave().GetProgress(activeWordPack);

		activeSpellPrefab = PlayerLoadoutController.s.GetSpellWithIndex(toActivateIndex);
		
		curWordCount = 0;
		ActivateWordMatchMode();
	}

	public void ActivateWordMatchMode() {
		SwitchWord(true);
		Time.timeScale = slowDownSpeed;
		animWordDisplay.SetBool("isWord", true);
		
		alreadyCastingSpellOverlay.SetActive(true);
	}

	public void StopWordMatchMode() {
		ClearState();
		animWordDisplay.ResetAllAnimatorTriggers();
		animWordDisplay.SetBool("isWord", false);
		//animWordDisplay.SetTrigger("reset");

		Time.timeScale = 1;
		
		alreadyCastingSpellOverlay.SetActive(false);
	}

	public void BeginTimedSection() {
		if(timerToStart != null)
			activeTimer = StartCoroutine(timerToStart);
	}

	public void SwitchWord(bool isFirstTime = false) {
		if(!isFirstTime)
			animWordDisplay.SetTrigger("refreshWord");
		
		curWordCount++;

		if (curWordCount >= wordCountPerPack) {
			if(activeSpell != null)
				activeSpell.DestroySelf();
			
			StopWordMatchMode();
			return;
		}

		
		activeWordPair = Scheduler.GetNextWordPair(activeWordPack, activeUserProgress, activeSide);

		StartCoroutine(DelayedChangeWord(activeWordPair));
		

		var userWordPairProgress = activeUserProgress.GetWordPairData(activeWordPair);
		
		if(activeSpell == null)
			activeSpell = Instantiate(activeSpellPrefab, spellParent).GetComponent<ISpell>();

		if (userWordPairProgress.type == 0) {
			NewWord();
		} else {
			StandardWordMatch();
		}
	}

	IEnumerator DelayedChangeWord(WordPair currentWord) {
		yield return new WaitForSeconds(wordChangeDelay*slowDownSpeed);
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

	public void NewWord() {
		ClearState();
		sliderFill.color = newColor;
		_inputChecker = NewWordInputChecker;
		statusText.text = "New Word";
		
		animWordDisplay.SetTrigger("newWord");

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
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, activeWordPair, activeSide, true);
		
		ClearState();
		sliderFill.color = correctColor;
		_inputChecker = TransitionInputChecker;
		statusText.text = "Correct!";
		
		animWordDisplay.SetTrigger("correct");
		
		
		ShootSpell();
		
		timerToStart = Timer(correctWordTimer, () => SwitchWord());
		BeginTimedSection();

		soundController.CorrectMatchSoundEffect();
	}
	
	public void OutOfTime() {
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, activeWordPair, activeSide, false);
		
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
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, activeWordPair, activeSide, false);
		
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
		time *= slowDownSpeed;
		var timer = time;

		while (timer > 0) {
			timeSlider.value = timer / time;
			timer -= Time.deltaTime;
			yield return null;
		}

		callback?.Invoke();
	}

	public SimpleEffectController[] ActiveLaneShower = new SimpleEffectController[2];

	public void ChangeShootLane(int side) {
		activeLane = MapController.s.myLanes[side];
		ActiveLaneShower[side].ActivateEffect();
		ActiveLaneShower[(side+1) % ActiveLaneShower.Length].DisableEffect();
	}

	public void ShootSpell() {
		activeSpell.Engage(activeLane, false, isAOE);
		activeSpell = null;
	}

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
		if (word == "fix") {
			FixWord();
			return;
		}
		
		if (IsMatch(word, meaningText.text)) {
			Scheduler.RegisterResult(activeWordPack, activeUserProgress, activeWordPair, activeSide, true);
			SwitchWord();
		} else {
			animWordDisplay.SetTrigger("wrong");
		}
	}

	public void StandardInputChecker(string word) {
		if (!IsEmpty(word)) {
			Debug.Log("DEBUG MATCHING OPTIONS PRESENT");
			if (word == "y" || word == "c") {
				CorrectMatch();
				return;
			} else if (word == "n" || word == "w") {
				WrongMatch();
				return;
			}

			if (word == "fix") {
				FixWord();
				return;
			}

			if (IsMatch(word, meaningText.text)) {
				CorrectMatch();
				return;
			} else {
				WrongMatch();
				return;
			}
		}
	}

	public WordFixOverlay wordFixOverlay;
	void FixWord() {
		wordFixOverlay.Engage(activeWordPair);
	}

	public void TransitionInputChecker(string word) {
		SwitchWord();
	}

}

public interface ISpell {
	void Engage(Lane lane, bool isComingFromLeft, bool isAOE);
	void DestroySelf();
}