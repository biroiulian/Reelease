using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClaimButtonController : MonoBehaviour
{
    public GameObject popupCanvas;

    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(ClaimCommand);
    }

    void ClaimCommand()
    {
        Debug.Log("You have clicked the button!");

        // Get what type of element to claim... difficulty-based + what the user already has in the inventory?

        // Add item to inventory

        // Show popup
        popupCanvas.SetActive(true);
    }
}
