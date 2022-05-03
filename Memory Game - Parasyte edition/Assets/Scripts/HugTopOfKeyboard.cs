using System.Collections;
using System.Collections.Generic;
using AdvancedInputFieldPlugin;
using UnityEngine;

public class HugTopOfKeyboard : MonoBehaviour
{
    
    private Canvas _canvas;

    private float minHeight = 0;
    public Canvas canvas
    {
        get
        {
            if(_canvas == null)
            {
                _canvas = GetComponentInParent<Canvas>();
            }

            return _canvas;
        }
    }
    
    void Start()
    {
#if(UNITY_ANDROID || UNITY_IOS)
        if(!Application.isEditor || Settings.SimulateMobileBehaviourInEditor)
        {
            NativeKeyboardManager.AddKeyboardHeightChangedListener(OnKeyboardHeightChanged);
        }
#endif
    }

    private void OnKeyboardHeightChanged(int keyboardHeight) {
        if (keyboardHeight > 0) {
            var rectTransform = GetComponent<RectTransform>();
            Vector2 position = rectTransform.anchoredPosition;
            position.y = keyboardHeight / canvas.scaleFactor;
            rectTransform.anchoredPosition = position;
        }
    }

}
