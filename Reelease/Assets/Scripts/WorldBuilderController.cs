using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldBuilderController : MonoBehaviour
{
    public GameState GameplayState;
    public MapController MapController;
    private Button NextButton;
    private Button AcceptButton;


    private void OnEnable()
    {
        NextButton = GetComponentsInChildren<Button>()[0];
        NextButton.onClick.AddListener(NextMapCommand);

        AcceptButton = GetComponentsInChildren<Button>()[1];
        AcceptButton.onClick.AddListener(AcceptMapCommand);
    }

    private void AcceptMapCommand()
    {
        // Save map to file 
        MapController.SaveMap();

    }

    private void NextMapCommand()
    {
        bool useRain = GetComponentsInChildren<Toggle>()[0].isOn;
        bool useGrass = GetComponentsInChildren<Toggle>()[1].isOn;

        MapController.noiseArgs.seed++;
        MapController.ComputeMap(useRain, useGrass);
        NextButton.gameObject.GetComponentsInChildren<TextMeshProUGUI>()[0].text = "Loading..."; 
    }

    private void Update()
    {
        if (MapController.MapComputationDone)
        {
            MapController.DrawMap();
            NextButton.gameObject.GetComponentsInChildren<TextMeshProUGUI>()[0].text = "Next";
            MapController.MapComputationDone = false;
        }
    }

}


