using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UsageTimeShow : MonoBehaviour
{
    public UsageTimePluginController controller;

    public void UpdateShowedUsageTime()
    {
        gameObject.GetComponent<TextMeshProUGUI>().text = ((int)controller.UsageTime) / 60 + " : " + ((int)controller.UsageTime) % 60;
    }

}
