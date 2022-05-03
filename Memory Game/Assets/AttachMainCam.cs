using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachMainCam : MonoBehaviour
{
    void Start()
    {
        GetComponent<Canvas>().worldCamera = SceneLoader.s.ScreenSpaceCanvasCamera;
    }

}
