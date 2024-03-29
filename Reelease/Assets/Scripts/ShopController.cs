using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct ShopItemsHolder
{
    public string[] items;
}

public class ShopController : MonoBehaviour
{
    public GameObject ItemsContainer;

    public GameObject ItemPrefab;

    public GameStateManager GameState;

    public ResourceDictionary ResourceDictionary;

    public InventoryController InventoryController;

    private ShopItemsHolder shopItemsHolder;

    private readonly string filePath = "/shopItems.json";

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        Debug.Log("Initialize Shop.");
        //
        ReadData();

        // Initialize - set individual things and show on the screen
        for (int i = 0; i < shopItemsHolder.items.Length; i++)
        {
            InitializeItem(shopItemsHolder.items[i]);
        }

    }

    private void InitializeItem(string itemType)
    {
        var item = Instantiate(ItemPrefab, ItemsContainer.transform);

        var itemInfo = ResourceDictionary.GetItemResource(itemType);

        // Get Text children's components
        var textsToSet = item.GetComponentsInChildren<TextMeshProUGUI>();
        // Get name with spaces
        textsToSet[0].text = GetBeautifulName(itemInfo.itemType);
        // Set price
        textsToSet[1].text = itemInfo.price.ToString();
        // Set icon
        var itemButton = item.GetComponentsInChildren<Button>()[0];
        itemButton.onClick.AddListener(() => BuyCommand(itemInfo));
        item.GetComponentsInChildren<Image>()[1].sprite = itemInfo.sprite;

    }

    private void BuyCommand(ItemResource info)
    {
        GameState.SetCoins(GameState.GetCoins() - info.price);
        InventoryController.AddItem(info);

    }

    private void ReadData()
    {
        if (JsonDataService.FileExists(filePath))
        {
            shopItemsHolder = JsonDataService.LoadData<ShopItemsHolder>(filePath);
            Debug.Log("Read " + shopItemsHolder.items.Length + " items");
        }
        else
        {
            Debug.Log("Trying to write default values");
            shopItemsHolder = FirstTimeInitialize();
            JsonDataService.SaveData(filePath, shopItemsHolder);
        }
    }

    private ShopItemsHolder FirstTimeInitialize()
    {
        return new ShopItemsHolder() { items = new string[] { "Fox", "Llama", "Pug", "Horse", "WhiteHorse", "Sheep", "Cow", "Stag","Donkey", "BirchTreeModel1", "BirchTreeModel2" } };
    }

    private string GetBeautifulName(ItemType t)
    {
        var enumName = t.ToString();
        // Insert spaces between words
        string spacedName = Regex.Replace(enumName, "(\\B[A-Z])", " $1");

        return spacedName;
    }
}
