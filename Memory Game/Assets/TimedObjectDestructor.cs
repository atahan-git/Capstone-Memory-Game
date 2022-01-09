using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedObjectDestructor : MonoBehaviour {

    public float timeOut = 2f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, timeOut);
    }
}
