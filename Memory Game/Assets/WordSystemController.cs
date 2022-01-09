using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordSystemController : MonoBehaviour {
	public string[] wordPackNames = new string[3];
	public GameObject[] spells = new GameObject[3];
	public Transform spellParent;
	
	
	
	public GameObject wordShowPanel;
	public TMP_Text wordText;
	public GameObject meaningShowPanel;
	public TMP_Text meaningText;
	public Slider timeSlider;
	public Image sliderFill;
	public GameObject blackBackOverlay;
	
	public Color regularColor = Color.cyan;
	public Color newColor = Color.yellow;
	public Color wrongColor = Color.red;
	public Color correctColor = Color.green;

	public Transform effectParent;
	public GameObject wrongEffect;
	public GameObject outOfTimeEffect;
	public GameObject correctEffect;
	public GameObject newWordEffect;

	public WordPack activeWordPack;
	public DataSaver.UserWordPackProgress activeUserProgress;
	public bool activeSide;
	public int currentWordIndex;
	public int activeWordPackIndex;
	public Lane activeLane;
	public ISpell activeSpell;
	public int curWordCount = 0;
	public bool isAOE = false;

	public float wordPickTimer = 5f;
	public float newWordTimer = 10f;
	public float wrongWordTimer = 10f;
	public float correctWordTimer = 1f;
	public int wordCountPerPack = 10;

	private void Start() {
		wordShowPanel.SetActive(false);
		meaningShowPanel.SetActive(false);
		blackBackOverlay.SetActive(false);
		ChangeShootLane(0);
		
		
		MobileKeyboardCapturer.s.StartListening(TryMatchWord);
	}

	public void ActivateWordPack(int buttonid) {
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
		}
		
		curWordCount = 0;
		ActivateWordMatchMode();
	}

	public void ActivateWordMatchMode() {
		wordShowPanel.SetActive(true);
		
		SwitchWord();
	}

	public void SwitchWord() {
		

		curWordCount++;

		if (curWordCount >= wordCountPerPack) {
			if(activeSpell != null)
				activeSpell.DestroySelf();
			wordShowPanel.SetActive(false);
			meaningShowPanel.SetActive(false);
			blackBackOverlay.SetActive(false);

			return;
		}
		
		currentWordIndex = Scheduler.GetNextWordPairIndex(activeWordPack, activeUserProgress, activeSide);

		var currentWord = activeWordPack.wordPairs[currentWordIndex];
		wordText.text = activeSide ? currentWord.word : currentWord.meaning;
		meaningText.text = !activeSide ? currentWord.word : currentWord.meaning;

		var userWordPairProgress = activeUserProgress.GetWordPairData(currentWordIndex);
		
		if(activeSpell == null)
			activeSpell = Instantiate(spells[activeWordPackIndex], spellParent).GetComponent<ISpell>();

		if (userWordPairProgress.type == 0) {
			NewWord();
		} else {
			StandardWordMatch();
		}
	}


	Callback skipAction;
	private Coroutine activeTimer;
	public void NewWord() {
		blackBackOverlay.SetActive(true);
		sliderFill.color = newColor;
		
		meaningShowPanel.SetActive(true);

		Instantiate(newWordEffect, effectParent);

		skipAction = StandardWordMatch;
		
		if(activeTimer != null)
			StopCoroutine(activeTimer);
		activeTimer = StartCoroutine(Timer(newWordTimer, null));
	}

	public void StandardWordMatch() {
		meaningShowPanel.SetActive(false);
		blackBackOverlay.SetActive(false);
		sliderFill.color = regularColor;

		skipAction = null;
		
		if(activeTimer != null)
			StopCoroutine(activeTimer);
		activeTimer = StartCoroutine(Timer(wordPickTimer, null));
	}

	public void CorrectMatch() {
		ShootSpell();
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, currentWordIndex, activeSide, true);
		
		blackBackOverlay.SetActive(false);
		sliderFill.color = correctColor;
		
		meaningShowPanel.SetActive(true);
		
		Instantiate(correctEffect, effectParent);
		
		skipAction = SwitchWord;
		
		if(activeTimer != null)
			StopCoroutine(activeTimer);
		activeTimer = StartCoroutine(Timer(correctWordTimer, SwitchWord));
	}
	
	public void OutOfTime() {
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, currentWordIndex, activeSide, false);
		
		blackBackOverlay.SetActive(true);
		sliderFill.color = wrongColor;
		
		meaningShowPanel.SetActive(true);
		
		Instantiate(outOfTimeEffect, effectParent);
		
		if(activeTimer != null)
			StopCoroutine(activeTimer);
		activeTimer = StartCoroutine(Timer(wrongWordTimer, SwitchWord));
	}

	public void WrongMatch() {
		Scheduler.RegisterResult(activeWordPack, activeUserProgress, currentWordIndex, activeSide, false);
		
		blackBackOverlay.SetActive(true);
		sliderFill.color = wrongColor;
		
		meaningShowPanel.SetActive(true);
		
		Instantiate(wrongEffect, effectParent);
		
		skipAction = SwitchWord;
		
		if(activeTimer != null)
			StopCoroutine(activeTimer);
		activeTimer = StartCoroutine(Timer(wrongWordTimer, SwitchWord));
	}
	
	delegate void Callback();

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

	public void ChangeShootLane(int side) {
		activeLane = MapController.s.myLanes[side];
		ActiveLaneShower[side].ActivateEffect();
		ActiveLaneShower[(side+1) % ActiveLaneShower.Length].DisableEffect();
	}

	public void ShootSpell() {
		activeSpell.Engage(activeLane, false, isAOE);
		activeSpell = null;
	}

	public void TryMatchWord(string word) {
		if (word.Replace(" ", "") == "") {
			if (skipAction != null) {
				if (activeTimer != null)
					StopCoroutine(activeTimer);
				skipAction.Invoke();
			}
			
		} else {
			if (StringDistance.LevenshteinDistance(word, meaningText.text) <= 1) {
				CorrectMatch();
			} else {
				WrongMatch();
			}
		}
	}
}


public abstract class WordMatchState {
	public float time = 5;
	public bool isTransitioning = true;
	
	public abstract void Initialize();

	private float timer;

	public void InitTimer() {
		timer = time;
	}

	public bool Timer(Slider slider) {
		slider.value = timer / time;
		timer -= Time.deltaTime;
		return timer > 0;
	}

	protected bool isMatch(string word1, string word2) {
		if (StringDistance.LevenshteinDistance(word1, word2) <= 1) {
			return true;
		} else {
			return false;
		}
	}

	public abstract WordMatchState ProcessInput(string input);
}

public interface ISpell {
	void Engage(Lane lane, bool isComingFromLeft, bool isAOE);
	void DestroySelf();
}