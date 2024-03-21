using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminContentCreator : MonoBehaviour
{
    [SerializeField]
    public ItemType shopItem;

    [SerializeField]
    public ChallengeItem challengeItem;

    public string shopFilePath = "/shopItems.json";
    public string challengeFilePath = "/challengeItems.json";
    public string inventoryItemsFilePath = "/inventoryItems.json";

    public void AddShopItem()
    {
        if (JsonDataService.FileExists(shopFilePath))
        {
            ShopItemsHolder itemsHolder = JsonDataService.LoadData<ShopItemsHolder>(shopFilePath);

            // Add the new item at the end of the array
            string[] items = new string[itemsHolder.items.Length + 1];
            Array.Copy(itemsHolder.items, items, itemsHolder.items.Length);
            items[items.Length - 1] = shopItem.ToString();

            // Save change locally
            itemsHolder.items = items;

            // Write to file
            JsonDataService.SaveData(shopFilePath, itemsHolder);
        }
        else
        {
            JsonDataService.SaveData(shopFilePath, new ShopItemsHolder { items = new string[] { ItemType.Fox.ToString() } });
        }
    }

    public void AddChallengeItem()
    {
        if (JsonDataService.FileExists(challengeFilePath))
        {
            ChallengeItemsHolder itemsHolder = JsonDataService.LoadData<ChallengeItemsHolder>(challengeFilePath);

            // Add the new item at the end of the array
            ChallengeItem[] items = new ChallengeItem[itemsHolder.items.Length + 1];
            Array.Copy(itemsHolder.items, items, itemsHolder.items.Length);
            items[items.Length - 1] = challengeItem;

            // Save change locally
            itemsHolder.items = items;

            // Write to file
            JsonDataService.SaveData(challengeFilePath, itemsHolder);
        }
        else
        {
            JsonDataService.SaveData(challengeFilePath, new ChallengeItemsHolder { items = new ChallengeItem[]
                { new ChallengeItem() { date = challengeItem.date, duration=challengeItem.duration, status = challengeItem.status, type = challengeItem.type } } 
            });
        }
    }

    public void AddInventoryItem()
    {
        throw new NotImplementedException();
    }
}
