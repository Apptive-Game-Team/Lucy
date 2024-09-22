using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public enum CameraEffectType
{
    NONE,
    FADE_OUT,
    FADE_IN,
}

public interface ICameraEffect
{
    void OnEffect();
}

public abstract class CameraEffect : ICameraEffect
{

    public CameraEffectType cameraEffectType;
    protected float duration;

    public CameraEffect(CameraEffectType type, float second)
    {
        cameraEffectType = type;
        duration = second;
    }

    public void OnEffect()
    {
        throw new System.NotImplementedException();
    }
}

public class FadeOutCameraEffect : CameraEffect
{
    public FadeOutCameraEffect(CameraEffectType type, float second) : base(type, second)
    {
    }
}

public class CameraEffector : MonoBehaviour
{
    Image fadeImage;
    const float FADE_DURATION = 5f;

    List<CameraEffect> cameraEffectList;

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

    public void AddCameraEffect(CameraEffect cameraEffect)
    {
        cameraEffectList.Add(cameraEffect);
        cameraEffect.OnEffect();
    }

}
