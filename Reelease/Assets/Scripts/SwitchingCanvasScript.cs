using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchingCanvasScript : MonoBehaviour
{
    [SerializeField]
    public Canvas CanvasToEnable;

    [SerializeField]
    public Canvas CanvasToDisable;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(SwitchCanvas);
    }

    private void SwitchCanvas()
    {
        CanvasToDisable.enabled = false;
        CanvasToEnable.enabled = true;
    }
}
