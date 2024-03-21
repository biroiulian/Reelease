using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public GameObject PlaceablesContainer;
    public GameObject PlaceablePrefab;
    public GameObject ConsumablesContainer;
    public GameObject ConsumablePrefab;

    [SerializeField]
    private ResourceDictionary ResourceDictionary;

    // TODO
    private GameObject[] placeables;
    private GameObject[] consumables;

    private string filePath = "/inventoryItems.json";

    private InventoryHolder inventory;

    void Start()
    {
        inventory = new InventoryHolder() { items = new string[0] };
        ReadData();
        Initialize();
    }

    private void ReadData()
    {
        if (JsonDataService.FileExists(filePath))
        {
            inventory = JsonDataService.LoadData<InventoryHolder>(filePath);
            // Debug.Log("Read " + inventory.items.Length + " items");
        }
        else
        {
            Debug.Log("Inventory empty... Can't read.");
        }
    }

    private void Initialize()
    {
        // Initialize - set individual things and show on the screen

            for (int i = 0; i < inventory.items.Length; i++)
            {
                InstantiateItem(inventory.items[i]);
            }
    }

    private void InstantiateItem(string itemType)
    {
        var itemInfo = ResourceDictionary.GetItemResource(itemType);

        if (itemInfo.itemGeneralType == ItemGeneralType.Placeable)
        {   
            var item = Instantiate(PlaceablePrefab);
            item.transform.parent = PlaceablesContainer.transform;

            // Get Text children's components
            var textsToSet = item.GetComponentsInChildren<TextMeshProUGUI>();
            // Get description
            textsToSet[0].text = GetBeautifulName(itemInfo.itemType);
            // Set icon
            item.GetComponentsInChildren<Image>()[1].sprite = itemInfo.sprite;

        }
        else if(itemInfo.itemGeneralType == ItemGeneralType.Consumable)
        {
            var item = Instantiate(ConsumablePrefab);
            item.transform.parent = ConsumablesContainer.transform;

            // Get Text children's components
            var textsToSet = item.GetComponentsInChildren<TextMeshProUGUI>();
            // Get description
            textsToSet[0].text = GetBeautifulName(itemInfo.itemType);
            // Set icon
            item.GetComponentsInChildren<Image>()[2].sprite = itemInfo.sprite;
        }
    }

    private void InstantiateItem(ItemResource itemInfo)
    {
        if (itemInfo.itemGeneralType == ItemGeneralType.Placeable)
        {
            var item = Instantiate(PlaceablePrefab);
            item.transform.parent = PlaceablesContainer.transform;

            // Get Text children's components
            var textsToSet = item.GetComponentsInChildren<TextMeshProUGUI>();
            // Get description
            textsToSet[0].text = GetBeautifulName(itemInfo.itemType);
            // Set icon
            item.GetComponentsInChildren<Image>()[1].sprite = itemInfo.sprite;

        }
        else if (itemInfo.itemGeneralType == ItemGeneralType.Consumable)
        {
            var item = Instantiate(ConsumablePrefab);
            item.transform.parent = ConsumablesContainer.transform;

            // Get Text children's components
            var textsToSet = item.GetComponentsInChildren<TextMeshProUGUI>();
            // Get description
            textsToSet[0].text = GetBeautifulName(itemInfo.itemType);
            // Set icon
            item.GetComponentsInChildren<Image>()[2].sprite = itemInfo.sprite;
        }
    }

    private string GetBeautifulName(ItemType item)
    {
        var enumName = item.ToString();
        // Insert spaces between words
        string spacedName = Regex.Replace(enumName, "(\\B[A-Z])", " $1");

        return spacedName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void AddItem(ItemResource info)
    {
        InstantiateItem(info);
        SaveNewItem(info);
    }

    private void SaveNewItem(ItemResource info)
    {
        if (JsonDataService.FileExists(filePath))
        {
            InventoryHolder invHolder = JsonDataService.LoadData<InventoryHolder>(filePath);

            // Add the new item at the end of the array
            string[] items = new string[invHolder.items.Length + 1];
            Array.Copy(invHolder.items, items, invHolder.items.Length);
            items[items.Length - 1] = info.itemType.ToString();

            // Save change locally
            inventory.items = items;

            // Write to file
            JsonDataService.SaveData(filePath, inventory);
        }
        else
        {
            JsonDataService.SaveData(filePath, new InventoryHolder { items = new string[] { info.itemType.ToString() } });
        }
    }
}

public struct InventoryHolder
{
    public string[] items;
}
