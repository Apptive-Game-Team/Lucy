using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FadeInTextTMP : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public float duration = 2f;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        Color textColor = tmpText.color;
        textColor.a = 0f;
        tmpText.color = textColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textColor.a = Mathf.Lerp(0f, 1f, elapsed / duration);
            tmpText.color = textColor;
            yield return null;
        }

        textColor.a = 1f;
        tmpText.color = textColor;
    }
}
