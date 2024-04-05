using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UserPlacedEnviromentController : MonoBehaviour
{
    private string filePath = "/userEnviroment.json";

    public ResourceDictionary ResourceDictionary;

    private List<EnviromentItem> userPlacedItems;

    public void OnApplicationFocus(bool focus)
    {
        if (focus == false)
        {
            Debug.Log("Application lost focus. Saving user placed enviroment positions.");
            SaveEnviroment();
        }

        if (focus == true)
        {
            Debug.Log("Application gained focus. Loading user placed enviroment positions.");
            LoadEnviroment();
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Application quit. Saving user placed enviroment positions.");
        SaveEnviroment();
    }

    public void AddUserPlacedItem(ItemResource itemInfo)
    {
        userPlacedItems.Add(new EnviromentItem() { });
    }

    public void LoadEnviroment()
    {
        Debug.Log("Loading animals, trees and other user-placed enviroment.");

        DeleteEnviroment();

        if (JsonDataService.FileExists(filePath))
        {
            var env = JsonDataService.LoadData<EnviromentData>(filePath);
            foreach (var item in env.items)
            {
                var instance = Instantiate(ResourceDictionary.GetItemResource(item.itemType.ToString()).placeablePrefab, transform);
                instance.name = item.itemType.ToString();
                instance.transform.position = new Vector3(item.position.x, item.position.y, item.position.z);
                instance.transform.rotation.eulerAngles.Set(item.rotation.x, item.rotation.y, item.rotation.z);
            }
        }
    }

    public void SaveEnviroment()
    {
        var items = gameObject.GetComponentsInChildren<Rigidbody>();
        Debug.Log("count of rigidbodies: " + items.Length);
        var toSaveItems = new List<EnviromentItem>();
        foreach (var item in items)
        {
            toSaveItems.Add(new EnviromentItem()
            {
                itemType = (ItemType)Enum.Parse(typeof(ItemType), item.gameObject.name),
                position = new Coords3D() { x = item.gameObject.transform.position.x, y = item.gameObject.transform.position.y, z = item.gameObject.transform.position.z },
                rotation = new Coords3D() { x = item.gameObject.transform.rotation.x, y = item.gameObject.transform.rotation.y, z = item.gameObject.transform.rotation.z },
            });
        }

        JsonDataService.SaveData(filePath, new EnviromentData() { items = toSaveItems.ToArray() });
    }

    private void DeleteEnviroment()
    {
        var items = gameObject.GetComponentsInChildren<Rigidbody>();
        foreach (var item in items)
        {
            DestroyImmediate(item.gameObject);
        }
    }
}
