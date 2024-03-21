using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OkPopupButton : MonoBehaviour
{

    private GameObject parentCanvas;
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(CloseCommand);
        parentCanvas = gameObject.transform.parent.transform.parent.gameObject;
    }

    void CloseCommand()
    {
        parentCanvas.SetActive(false);
    }
}
