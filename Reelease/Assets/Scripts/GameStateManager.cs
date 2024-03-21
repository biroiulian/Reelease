using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GameStateManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI coinsCounter;

    [SerializeField]
    private TextMeshProUGUI streakCounter;

    public void SetCoins(int value)
    {
        // Save change
        gameState.Coins = value;
        JsonDataService.SaveData(filePath, gameState);
        UpdateCounters();
    }
    public int GetCoins()
    {
        return gameState.Coins;
    }

    public int Streak;
    public void SetStreak(int value)
    {
        // Save change
        Streak = value;
        gameState.Streak = value;
        JsonDataService.SaveData(filePath, gameState);
        UpdateCounters();
    }
    public int GetStreak()
    {
        return Streak;
    }

    public int TimeSpentToday { private set; get; }

    private readonly string filePath = "/gameState.json";
    private GameState gameState;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    private void UpdateCounters()
    {
        // Update display
        coinsCounter.text = gameState.Coins.ToString();
        streakCounter.text = gameState.Streak.ToString();
    }

    private void Initialize()
    {
        // Read from json
        if (JsonDataService.FileExists(filePath))
        {
                gameState = JsonDataService.LoadData<GameState>(filePath);
                UpdateCounters();
        }
        else
        {
            Debug.Log("gameState: First time init.");
            gameState = FirstTimeInitialize();
            JsonDataService.SaveData(filePath, gameState);
            UpdateCounters(); 
        }
    }

    private GameState FirstTimeInitialize()
    {
        return new GameState() { Coins = 200, Streak = 20 };
    }
}

public struct GameState
{
    public int Coins;
    public int Streak;
}
