using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// I need to define the behaviour of this class. How will the challenge system actually work.
/// Ideea: if a certain CheckChallenges method is called everytime the app starts, and that
/// method finds that the last Date that the ChallengeItemsHolder was Checked is yesterday, then it
/// starts iterating through the items, to see if the screentime/duration computation (done somewhere else)
/// is smaller than the duration of each item. If it is, make the challenge claimable. The button should
/// get activated.
/// Also if the date is the same, you can check for challenges to see if they are still accomplishable.
/// Like if screentime = 120 and challenge duration = 60. You can't make 60, can't turn back the time.
/// Then, make the challenge Failed.
/// </summary>

public class ChallengesController : MonoBehaviour
{
    public GameObject ItemsContainer;
    public GameStateManager GameState;
    public GameObject ChallengeItemPrefab;
    public ResourceDictionary ResourceDictionary;
    public GameObject PopupCanvas;

    private ChallengeItemsHolder challengeItemsHolder;
    private string filePath = "/challengeItems.json";

    // might need a Prefab[] array to easily update our buttons

    // Start is called before the first frame update
    void Start()
    {
        ReadData();
        UpdateChallenges();

        Initialize();
    }

    private void Initialize()
    {
        // Initialize - set individual things and show on the screen
        for (int i = 0; i < challengeItemsHolder.items.Length; i++)
        {
            InitializeItem(challengeItemsHolder.items[i]);
        }

    }

    private void InitializeItem(ChallengeItem challenge)
    {
        var item = Instantiate(ChallengeItemPrefab);
        item.transform.parent = ItemsContainer.transform;

        // Get Text children's components
        var textsToSet = item.GetComponentsInChildren<TextMeshProUGUI>();
        // Get description
        textsToSet[0].text = GetDescription(challenge);
        // Set Button text
        textsToSet[1].text = challenge.status.ToString();
        // Set button active status
        item.GetComponentsInChildren<Button>()[0].interactable = challenge.status == ChallengeStatus.Success;
        // Set icon
        item.GetComponentsInChildren<Image>()[1].sprite = ResourceDictionary.GetIconFor(challenge);
        // Assign gameState
        item.GetComponentsInChildren<ClaimButtonController>()[0].popupCanvas = PopupCanvas;
    }

    private string GetDescription(ChallengeItem challenge)
    {
        string type = Regex.Replace(challenge.type.ToString(), "(\\B[A-Z])", " $1");
        var desc = "Keep " + type + " time below " + challenge.duration + " minutes!"; 
        return desc;
    }

    private void ReadData()
    {
        if (JsonDataService.FileExists(filePath))
        {
            challengeItemsHolder = JsonDataService.LoadData<ChallengeItemsHolder>(filePath);
            Debug.Log("Read " + challengeItemsHolder.items.Length + " items");
        }
        else
        {
            Debug.Log("Trying to write default values");
            challengeItemsHolder = FirstTimeInitialize();
            JsonDataService.SaveData(filePath, challengeItemsHolder);
        }
    }

    private void UpdateChallenges()
    {

    }

    private ChallengeItemsHolder FirstTimeInitialize()
    {
        return new ChallengeItemsHolder() { items = new ChallengeItem[] 
        { 
            new ChallengeItem() { status = ChallengeStatus.InProgress, duration = 15, type= ChallengeType.SocialMedia, date = DateTime.Now.ToString("dd/MM/yyyy") },
            new ChallengeItem() { status = ChallengeStatus.Success, duration = 30, type= ChallengeType.SocialMedia, date = DateTime.Now.ToString("dd/MM/yyyy") },
            new ChallengeItem() { status = ChallengeStatus.Failed, duration = 45, type= ChallengeType.SocialMedia, date = DateTime.Now.ToString("dd/MM/yyyy") },
        } 
        };
    }
}

public struct ChallengeItemsHolder
{
    public ChallengeItem[] items;
    
}

public struct ChallengeItem
{
    public ChallengeType type;
    public int duration; 
    public ChallengeStatus status;
    public string date;   // dd/MM/yyyy format
}

public enum ChallengeType
{
    SocialMedia,
    Screen
}

public enum ChallengeStatus
{
    InProgress,
    Success,
    Failed,
    Claimed
}