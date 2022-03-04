using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorCallback : MonoBehaviour
{

    public UnityEvent event1 = new UnityEvent();

    public void AnimationCallback1() {
        //event1.Invoke();
    }
}
