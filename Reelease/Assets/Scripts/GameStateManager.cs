using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.Rendering.DebugUI;

public class GameStateManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI coinsCounter;

    [SerializeField]
    private TextMeshProUGUI streakCounter;

    [SerializeField]
    private bool mapExists;

    public bool MapExists()
    {
        return mapExists;
    }

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

    public UnityEvent LoadedGameStateEvent;
    public UnityEvent NoMapFoundEvent;
    public UnityEvent MapFoundEvent;

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
            Debug.Log("GameStateManager: First time init.");
            gameState = FirstTimeInitialize();
            JsonDataService.SaveData(filePath, gameState);
            UpdateCounters();
        }

        if (!gameState.MapExists)
        {
            NoMapFoundEvent.Invoke();
        }
        else
        {
            MapFoundEvent.Invoke();
        }

        LoadedGameStateEvent.Invoke();
    }

    private GameState FirstTimeInitialize()
    {
        return new GameState() { Coins = 200, Streak = 0, MapExists=false };
    }

    public void SetMapExists()
    {
        gameState.MapExists = true;
        JsonDataService.SaveData(filePath, gameState);
    }
}

public struct GameState
{
    public bool MapExists;
    public int Coins;
    public int Streak;
}
