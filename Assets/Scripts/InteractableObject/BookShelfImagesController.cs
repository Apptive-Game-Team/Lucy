using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookShelfImagesController : MonoBehaviour
{
    public List<Image> images = new();
    private int currentActiveIndex = 0;

    public void SetActiveImage(int index)
    {
        foreach (var img in images)
        {
            img.gameObject.SetActive(false);
        }

        if (index >= 0 && index < images.Count)
        {
            images[index].gameObject.SetActive(true);
            currentActiveIndex = index;
        }
    }

    public void OnLeftButtonClick()
    {
        if (currentActiveIndex > 0)
        {
            SetActiveImage(currentActiveIndex - 1);
        }
    }

    public void OnRightButtonClick()
    {
        if (currentActiveIndex < images.Count - 1)
        {
            SetActiveImage(currentActiveIndex + 1);
        }
    }
}
