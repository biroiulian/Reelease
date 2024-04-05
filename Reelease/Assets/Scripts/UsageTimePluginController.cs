using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

public class UsageTimePluginController : MonoBehaviour
{
    AndroidJavaClass unityClass;
    AndroidJavaObject unityActivity;
    AndroidJavaObject unityContext;
    AndroidJavaObject _pluginInstance;

    public UnityEvent UsageTimeUpdated;
    public UnityEvent PermissionNotGranted;
    public UnityEvent PermissionGranted;
    private bool mapIsBuilt = false;
    public int UsageTime;
    public int PreviousDayUsageTime;

    // Start is called before the first frame update
    void Start()
    {
        UsageTime = 0;
        Debug.Log("Started in script PluginInit.");
        InitializePlugin("com.free.unity.TimeUsagePlugin");

        if (!HavePermission() && mapIsBuilt)
        {
            PermissionNotGranted.Invoke();
        }

        StartCoroutine(UpdateUsageTime());
    }

    public void SetMapIsBuilt()
    {
        mapIsBuilt = true;
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus && mapIsBuilt)
        {
            if (!HavePermission())
            {
                PermissionNotGranted.Invoke();
            }
            else
            {
                PermissionGranted.Invoke();
            }
        }
    }

    public void AskForPermission()
    {
        if (_pluginInstance != null)
        {
            try
            {
                _pluginInstance.Call("AskPermission");
            }
            catch (Exception e)
            {
                Debug.Log("unityerror haveperm " + e.Message);
            }
        }
    }

    private bool HavePermission()
    {
        var permissionInt = -1;
        if (_pluginInstance != null)
        {
            try
            {
                permissionInt = _pluginInstance.Call<int>("HavePermission");
            }
            catch (Exception e)
            {
                Debug.Log("unityerror haveperm " + e.Message);
            }

            if (permissionInt == 1)
            {
                Debug.Log("We have permission!");
                return true;
            }
            else
            {
                Debug.Log("We don't have permission!");
                PermissionNotGranted.Invoke();
                return false;
            }
        }

        return false;

    }

    private void InitializePlugin(string pluginName)
    {
        unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        unityContext = unityActivity.Call<AndroidJavaObject>("getApplicationContext");
        _pluginInstance = new AndroidJavaObject(pluginName);

        if (_pluginInstance == null)
        {
            Debug.Log("Plugin Instance Error");
        }

        if (unityContext is null)
        {
            Debug.Log("Unity context is null for some reason.");
        }

        _pluginInstance.CallStatic("receiveUnityContext", unityContext);
        _pluginInstance.CallStatic("receiveUnityActivity", unityActivity);


        Debug.Log("Finished initializing plugin...");
    }

    public void Toast()
    {
        Debug.Log("Calling Toast method");

        if (_pluginInstance != null)
        {
            _pluginInstance.Call("Toast", "This does nothing! Congratulations!");
        }
    }

    public IEnumerator UpdateUsageTime()
    {
        while (true)
        {
            var previousDayUsageTime = -1;
            var currentDayUsageTime = -1;
            var resultPermission = -1;
            try
            {
                resultPermission = _pluginInstance.Call<int>("HavePermission");
                Debug.Log("Permission status received is : " + resultPermission);

                if (resultPermission > 0)
                {
                    Debug.Log("Looks like we got permission. " + resultPermission);
                }
                else
                {
                    Debug.Log("Looks like we don't have permission. Too bad.");
                }

                // call for last day
                previousDayUsageTime = _pluginInstance.Call<int>("GetSocialMediaTime", 1);
                currentDayUsageTime = _pluginInstance.Call<int>("GetSocialMediaTime", 0);

            }
            catch (Exception e)
            {
                Debug.Log("unityerror " + e.Message);
            }

            if (previousDayUsageTime is -1 || currentDayUsageTime is -1)
            {
                Debug.Log("Returned usage time problem.");
            }
            else
            {

                Debug.Log("Returned usage time allright. Previous day in minutes: " + previousDayUsageTime/60);
                Debug.Log("Returned usage time allright. Today in minutes: " + currentDayUsageTime/60);
                UsageTime = currentDayUsageTime;
                PreviousDayUsageTime = previousDayUsageTime;
                UsageTimeUpdated.Invoke();
            }

            yield return new WaitForSeconds(2f);
        }
    }

}
