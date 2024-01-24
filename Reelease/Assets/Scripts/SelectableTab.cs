using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableTab : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    List<GameObject> Tabs;

    public Sprite UnselectedSprite;
    public Sprite HoverSprite;
    public Sprite SelectedSprite;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gameObject.GetComponent<Image>().sprite == UnselectedSprite ||
            gameObject.GetComponent<Image>().sprite == HoverSprite)
        {
            foreach (var tab in Tabs)
            {
                if (tab.GetComponent<Image>().sprite == SelectedSprite) 
                { 
                    tab.GetComponent<Image>().sprite = UnselectedSprite;
                    tab.transform.SetAsFirstSibling();
                }
            }
            gameObject.GetComponent<Image>().sprite = SelectedSprite;
            gameObject.transform.SetAsLastSibling();

        }
        else
        {
            // Do nothing
        }
    }

}
