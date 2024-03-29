using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldBuilderController : MonoBehaviour
{
    public GameState GameplayState;
    public MapController MapController;

    private void OnEnable()
    {
        GetComponentsInChildren<Button>()[0].onClick.AddListener(NextMapCommand);
        GetComponentsInChildren<Button>()[1].onClick.AddListener(AcceptMapCommand);
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

        MapController.seed++;
        MapController.GenerateMap(useRain, useGrass);
    }

}


