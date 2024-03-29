using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingIcon : MonoBehaviour
{
    public Sprite LoadingImage;
    public float RotationSpeed;

    private void Awake()
    {
        GetComponent<Image>().sprite = LoadingImage;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.RotateAround(transform.position, Vector3.forward, RotationSpeed * Time.deltaTime);
    }
}
