using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityRawInput;

public class KeySupressor : MonoBehaviour {

	public List<RawKey> KeysToSuppress = new List<RawKey>();

	private int _charges = 0;

	public TMP_Text chargesText;

	public RandomClipPlayer noChargeClip;
	public int Charges {
		get {
			return _charges;
		}

		set {
			int delta = value - _charges;
			_charges = value;
			if (_charges <= 0) {
				_charges = 0;
			}
			UpdateState(delta > 0);
		}
	}

	private void Start() {
		RawKeyInput.Start(true);
		RawKeyInput.KeysToIntercept = KeysToSuppress;
		RawKeyInput.InterceptMessages = false;
		RawKeyInput.OnKeyDown += HandleKeyDown;
		
		UpdateState(true);
	}

	private void UpdateState(bool isPositive) {
		//RawKeyInput.InterceptMessages = Charges <= 0;
		chargesText.text = Charges.ToString();
		

		if (!isPositive) {
			if (Charges <= 0) {
				noChargeClip.PlayDeathClip();
			} else if (Charges < 3) {
				noChargeClip.PlayGetHitClip();
			}
		} else {
			noChargeClip.PlayAttackClip();
		}
	}

	public void AddCharges() {
		Charges += 4;
	}

	private void HandleKeyDown(RawKey obj) {
		if (KeysToSuppress.Contains(obj)) {
			if(!Application.isFocused)
				Charges -= 1;
		}
	}


	private void OnDisable() {
		RawKeyInput.OnKeyDown -= HandleKeyDown;
		RawKeyInput.Stop();
	}
}