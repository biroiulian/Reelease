using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public class JsonDataService : IDataService
{
    private static string persistentPath;
    public JsonDataService() {
        persistentPath = Application.persistentDataPath;
    }

    public void SaveData<T>(string relativePath, T data, bool encrypted)
    {
        var path = persistentPath + relativePath;
        try
        {
            if (File.Exists(path))
            {
                Debug.Log("Data exists. Deleting old file and writing a new one!");
                File.Delete(path);
            }
            else
            {
                Debug.Log("Writing file for the first time!");
            }
            using FileStream stream = File.Create(path);    
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
           
        }
        catch (Exception e)
        {
            Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
            
        }
    }
    public T LoadData<T>(string relativePath, bool encrypted)
    {
        string path = persistentPath + relativePath;

        if (!File.Exists(path))
        {
            Debug.LogError($"Cannot load file at {path}. File does not exist!");
            throw new FileNotFoundException($"{path} does not exist!");
        }

        try
        {
            T data;
            data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
            throw e;
        }
    }

    public bool FileExists(string relativePath) {
        var path = Application.persistentDataPath + relativePath;
        return File.Exists(path);
    }

}