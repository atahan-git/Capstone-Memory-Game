//using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods {

    public static void ResetTransformation (this Transform trans) {
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = new Vector3(1, 1, 1);
    }

    public static Vector3 vector3 (this Vector2 v2, float z) {
        return new Vector3(v2.x, v2.y, z);
    }

    public static Vector2 vector2 (this Vector3 v3) {
        return new Vector2(v3.x, v3.y);
    }


    public static void DeleteAllChildren(this Transform transform) {
        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--) {
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }


    public static void InsertWithNullFill<T>(this List<T> ls, int index, T item) where T: class {
        while (!(index < ls.Count)) {
            ls.Add(null);
        }
        
        ls.Insert(index, item);
    }

    
    public static void ResetAllAnimatorTriggers(this Animator animator)
    {
        foreach (var trigger in animator.parameters)
        {
            if (trigger.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(trigger.name);
            }
        }
    }
    
 
    public static float Remap (this int value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    
    public static float Remap (this float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
    
    public static void SmartDestroy(this GameObject target) {
        if (target != null) {
            var particles = target.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < particles.Length; i++) {
                particles[i].Stop();
                Object.Destroy(particles[i].gameObject, 3f);
                particles[i].transform.SetParent(null);
            }

            Object.Destroy(target);
        }
    }
    
    public static void SetParticleSystems(this GameObject target, bool status) {
        if (target != null) {
            var particles = target.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < particles.Length; i++) {
                if(status)
                    particles[i].Play();
                else 
                    particles[i].Stop();
            }
        }
    }
}