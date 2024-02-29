using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public struct TabMenuPair
{
    public GameObject Tab;
    public GameObject Menu;
}

public class TabbingManager : MonoBehaviour
{
    [SerializeField]
    public TabMenuPair[] TabMenuPairs;
    public Sprite SelectedSpriteTab;
    public Sprite HoverSpriteTab;
    public Sprite UnselectedSpriteTab;


    void Start()
    {
        foreach (var pair in TabMenuPairs)
        {
            pair.Tab.GetComponent<Button>().onClick.AddListener(() => OnTabClick(pair));
            pair.Menu.SetActive(false);
        }

        // Set only the first one active
        TabMenuPairs[0].Menu.SetActive(true);
    }

    void OnTabClick(TabMenuPair selectedPair)
    {
        Debug.Log("Clicked the button");
        if (selectedPair.Tab.GetComponent<Image>().sprite == UnselectedSpriteTab ||
            selectedPair.Tab.GetComponent<Image>().sprite == HoverSpriteTab)
        {
            foreach (var pair in TabMenuPairs)
            {
                if (pair.Tab.GetComponent<Image>().sprite == SelectedSpriteTab)
                {
                    pair.Tab.GetComponent<Image>().sprite = UnselectedSpriteTab;
                    pair.Tab.transform.SetAsFirstSibling();
                    pair.Menu.SetActive(false);
                }
            }
            selectedPair.Tab.GetComponent<Image>().sprite = SelectedSpriteTab;
            selectedPair.Tab.transform.SetAsLastSibling();
            selectedPair.Menu.SetActive(true);

        }
        else
        {
            // Do nothing
        }
    }
}
