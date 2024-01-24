using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpenMenu : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    public Canvas CanvasToOpen;

    [SerializeField]
    public Image BlackAlphaFilterForBackground;

    public void OnPointerClick(PointerEventData eventData)
    {
        TryOpenMenuCanvas();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void TryOpenMenuCanvas()
    {
        Debug.Log("Opening menu canvas");
        if (!CanvasToOpen.isActiveAndEnabled) 
        {
            CanvasToOpen.enabled = true;
            CanvasToOpen.gameObject.SetActive(true);
            BlackAlphaFilterForBackground.gameObject.SetActive(true);
            BlackAlphaFilterForBackground.enabled = true;
        }
        else
        {
            CanvasToOpen.enabled = false;
            CanvasToOpen.gameObject.SetActive(false);
            BlackAlphaFilterForBackground.enabled = false;
            BlackAlphaFilterForBackground.gameObject.SetActive(false);


        }
    }
}
