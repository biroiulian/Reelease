using UnityEngine;

public class CanvasOpenClose : MonoBehaviour
{
    public void OpenCanvas() {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        GetComponent<CanvasGroup>().alpha = 1.0f;
        GetComponent<CanvasGroup>().interactable = true;
    }

    public void CloseCanvas()
    {
        GetComponent<CanvasGroup>().alpha = 0.0f;
        GetComponent<CanvasGroup>().interactable = false;
        gameObject.SetActive(false);
    }

}
