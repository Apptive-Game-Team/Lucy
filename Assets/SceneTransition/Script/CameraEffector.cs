using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class CameraEffector : MonoBehaviour
{
    Image fadeImage;

    const float FADE_DURATION = 5f;

    float curTime = 0;
    float curAlpha = 0;

    private void Start()
    {
        fadeImage = GetComponent<Image>();
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        curTime = 0;

        while (curTime < FADE_DURATION)
        {
            yield return null;
            curTime += Time.deltaTime;
            curAlpha = curTime / FADE_DURATION;
            fadeImage.color = new Color(0, 0, 0, curAlpha);
        }
    }
    public IEnumerator FadeIn()
    {
        curTime = FADE_DURATION;

        while (curTime > 0)
        {
            yield return null;
            curTime -= Time.deltaTime;
            curAlpha = curTime / FADE_DURATION;
            fadeImage.color = new Color(0, 0, 0, curAlpha);
        }
    }

}
