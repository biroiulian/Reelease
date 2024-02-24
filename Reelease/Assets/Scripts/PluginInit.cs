using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PluginInit : MonoBehaviour
{
    AndroidJavaClass unityClass;
    AndroidJavaObject unityActivity;
    AndroidJavaObject _pluginInstance;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started in script PluginInit.");
        // InitializePlugin("com.reelease.testlibrary.PluginInstance");
    }

    private void InitializePlugin(string pluginName)
    {
        unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        _pluginInstance = new AndroidJavaObject(pluginName);
        if (_pluginInstance == null)
        {
            Debug.Log("Plugin Instance Error");
        }
        _pluginInstance.CallStatic("receiveUnityActivity", unityActivity);
        Debug.Log("Finished initializing plugin...");
    }

    public void Add()
    {
        Debug.Log("Calling Add method");

        if (_pluginInstance != null)
        {
            var result = _pluginInstance.Call<int>("Add", 5, 6);
            Debug.Log("Result from plugin is : " + result);
        }

        Toast();
    }

    public void Toast()
    {
        Debug.Log("Calling Toast method");

        if (_pluginInstance !=null)
        {
            _pluginInstance.Call("Toast", "Hi from unity!");
        }
    }
}
