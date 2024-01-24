using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainBackgroundFadeIn : MonoBehaviour
{
    [SerializeField] 
    public Image StartBackground;

    [SerializeField]
    public Text StartText;

    [SerializeField]
    public float FadeTime = 0.7f;
    
    // Start is called before the first frame update
    void Start()
    {
        Invoke("TransitionToMainBackground", 2);
    }

    public void TransitionToMainBackground()
    {
        LeanTween.alpha(StartBackground.rectTransform, 0f, FadeTime).setEase(LeanTweenType.linear)
            .setOnComplete(
            () => { gameObject.SetActive(false); });
    }
}
