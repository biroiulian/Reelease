using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public static class JsonDataService
{
    private static readonly string persistentPath = Application.persistentDataPath;

    public static void SaveData<T>(string relativePath, T data, bool encrypted = false)
    {
        Debug.Log("Default data path: " + Application.persistentDataPath);
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
    public static T LoadData<T>(string relativePath, bool encrypted = false)
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

    public static bool FileExists(string relativePath) {
        var path = Application.persistentDataPath + relativePath;
        return File.Exists(path);
    }

}