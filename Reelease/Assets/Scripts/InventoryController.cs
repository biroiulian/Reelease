using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public GameObject PlaceablesContainer;
    public GameObject PlaceablePrefab;
    public GameObject ConsumablesContainer;
    public GameObject ConsumablePrefab;

    public Canvas MenuCanvasToHide;
    public Canvas MainCanvasToHide;
    public Canvas PlacingCanvasToShow;

    [SerializeField]
    private ResourceDictionary ResourceDictionary;

    // TODO
    private List<GameObject> placeables = new List<GameObject>();
    private List<GameObject> consumables = new List<GameObject>();

    private string filePath = "/inventoryItems.json";

    private InventoryHolder inventory;
    private ItemResource focusedElement;

    void Awake()
    {
        inventory = ReadData();
        InitializeItems();

        PlacingCanvasToShow.gameObject.GetComponentsInChildren<Button>()[0].onClick.AddListener(CancelPlacingCommand);
        PlacingCanvasToShow.gameObject.GetComponentsInChildren<Button>()[1].onClick.AddListener(ApplyPlacingCommand);
    }

    private InventoryHolder ReadData()
    {
        if (JsonDataService.FileExists(filePath))
        {
            var inventory = JsonDataService.LoadData<InventoryHolder>(filePath);

            if (inventory.items is null)
            {
                inventory.items = new string[0];
            }

            return inventory;
        }
        else
        {
            Debug.Log("First time reading the inventory. Adding a fox.");
            return new InventoryHolder() { items = new string[] { "Fox" } };
        }
    }

    private void InitializeItems()
    {
        // Initialize - set individual things and show on the screen
        Debug.Log("Initialize invetory.");
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
            // Set button action
            item.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => PlaceCommand(itemInfo));

            placeables.Add(item);

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

            consumables.Add(item);
        }
    }

    private void PlaceCommand(ItemResource itemInfo)
    {
        focusedElement = itemInfo;

        MenuCanvasToHide.GetComponent<CanvasGroup>().alpha = 0;
        MenuCanvasToHide.GetComponent<CanvasGroup>().blocksRaycasts = false;
        MenuCanvasToHide.GetComponent<CanvasGroup>().interactable = false;
        MainCanvasToHide.GetComponent<CanvasGroup>().alpha = 0;
        MainCanvasToHide.GetComponent<CanvasGroup>().blocksRaycasts = false;
        MainCanvasToHide.GetComponent<CanvasGroup>().interactable = false;

        PlacingCanvasToShow.gameObject.SetActive(true);

        if (itemInfo.itemType.ToString().Contains("Tree"))
        {
            PlacingCanvasToShow.gameObject.GetComponent<Placing>().StartPlacingEnviroment(itemInfo);
        }
        else
        {
            PlacingCanvasToShow.gameObject.GetComponent<Placing>().StartPlacingAnimal(itemInfo);
        }
    }

    private void ApplyPlacingCommand()
    {
        // Delete element from inventory
        for (int i = 0; i < placeables.Count; i++)
        {
            if (placeables[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text == GetBeautifulName(focusedElement.itemType))
            {
                Destroy(placeables[i]);
                placeables.RemoveAt(i);
                break;
            }
        }

        RemoveItemAndSave(focusedElement);

        // Close placing canvas and reopen menu and main
        MenuCanvasToHide.GetComponent<CanvasGroup>().alpha = 1;
        MenuCanvasToHide.GetComponent<CanvasGroup>().blocksRaycasts = true;
        MenuCanvasToHide.GetComponent<CanvasGroup>().interactable = true;
        MainCanvasToHide.GetComponent<CanvasGroup>().alpha = 1;
        MainCanvasToHide.GetComponent<CanvasGroup>().blocksRaycasts = true;
        MainCanvasToHide.GetComponent<CanvasGroup>().interactable = true;

        PlacingCanvasToShow.gameObject.SetActive(false);


    }

    private void RemoveItemAndSave(ItemResource gameObject)
    {
        // Find item position
        var count = 0;
        foreach (var item in inventory.items)
        {
            if (gameObject.itemType.ToString() == item)
            {
                break;
            }
            count++;
        }

        // Create a new array with one less element
        string[] newArray = new string[inventory.items.Length - 1];

        // Copy elements before the position
        Array.Copy(inventory.items, 0, newArray, 0, count);

        // Copy elements after the position
        Array.Copy(inventory.items, count + 1, newArray, count, inventory.items.Length - count - 1);

        // Update the original array reference
        inventory.items = newArray;

        JsonDataService.SaveData(filePath, inventory);

    }

    private void CancelPlacingCommand()
    {
        // Close placing canvas and reopen menu and main
        MenuCanvasToHide.GetComponent<CanvasGroup>().alpha = 1;
        MenuCanvasToHide.GetComponent<CanvasGroup>().blocksRaycasts = true;
        MenuCanvasToHide.GetComponent<CanvasGroup>().interactable = true;
        MainCanvasToHide.GetComponent<CanvasGroup>().alpha = 1;
        MainCanvasToHide.GetComponent<CanvasGroup>().blocksRaycasts = true;
        MainCanvasToHide.GetComponent<CanvasGroup>().interactable = true;

        PlacingCanvasToShow.gameObject.SetActive(false);
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
            // Set button action
            item.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => PlaceCommand(itemInfo));

            placeables.Add(item);

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

            consumables.Add(item);
        }
    }

    private string GetBeautifulName(ItemType item)
    {
        var enumName = item.ToString();
        // Insert spaces between words
        string spacedName = Regex.Replace(enumName, "(\\B[A-Z])", " $1");

        return spacedName;
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
            // Add the new item at the end of the array
            string[] items = new string[inventory.items.Length + 1];
            Array.Copy(inventory.items, items, inventory.items.Length);
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
