using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolowKeyboardHeight : MonoBehaviour {

    public float delta = 20f;

    public float lerpSpeed = 20;

    private Vector3 offset;
    // Start is called before the first frame update
    void Start() {
        offset = transform.position;
    }

    // Update is called once per frame
    void Update() {
        var targetPos = offset + Vector3.up * MobileUtilities.GetKeyboardHeightRatio(true) * delta;
        transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);
    }
}
