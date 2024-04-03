using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class UserPlacedEnviromentController : MonoBehaviour
{
    private string filePath = "/userEnviroment.json";

    public ResourceDictionary ResourceDictionary;

    public void OnApplicationFocus(bool focus)
    {
        if(focus == false)
        {
            SaveEnviroment();
        }

        if(focus == true)
        {
            LoadEnviroment();
        }
    }

    private void OnApplicationQuit()
    {
        SaveEnviroment();
    }


    public void LoadEnviroment()
    {
        Debug.Log("Loading animals, trees and other user-placed enviroment.");

        if (JsonDataService.FileExists(filePath))
        {
            var env = JsonDataService.LoadData<EnviromentData>(filePath);
            foreach (var item in env.items)
            {
                var instance = Instantiate(ResourceDictionary.GetItemResource(item.itemType.ToString()).placeablePrefab);
                instance.name = item.itemType.ToString();
                instance.transform.position = new Vector3(item.position.x, item.position.y, item.position.z);
                instance.transform.rotation.eulerAngles.Set(item.rotation.x, item.rotation.y, item.rotation.z);
            }
        }
    }

    public void SaveEnviroment()
    {
        var items = gameObject.GetComponentsInChildren<Transform>();
        var toSaveList = new List<EnviromentItem>();
        foreach (var item in items)
        {
            if (item != transform) //except the container itself 
            {
                toSaveList.Add(new EnviromentItem()
                {
                    itemType = (ItemType)Enum.Parse(typeof(ItemType), item.gameObject.name),
                    position = new Coords3D() { x = item.transform.position.x, y = item.transform.position.y, z = item.transform.position.z },
                    rotation = new Coords3D() { x = item.transform.rotation.x, y = item.transform.rotation.y, z = item.transform.rotation.z },
                });
            }
        }

        JsonDataService.SaveData(filePath, new EnviromentData() { items = toSaveList.ToArray() });
    }
}
