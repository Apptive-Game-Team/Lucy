using System.Collections;
using System.Collections.Generic;
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

public class CameraEffector : SingletonObject<CameraEffector>
{
    Image fadeImage;
    const float FADE_DURATION = 2f;

    List<CameraEffect> cameraEffectList;

    float curTime = 0;
    float curAlpha = 0;

    private void Start()
    {
        fadeImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
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

    const float TEMP_WAIT_SECOND = 0.5f;

    public IEnumerator FadeOutIn()
    {
        yield return FadeOut();
        yield return new WaitForSeconds(TEMP_WAIT_SECOND);
        yield return FadeIn();
    }

    public void AddCameraEffect(CameraEffect cameraEffect)
    {
        cameraEffectList.Add(cameraEffect);
        cameraEffect.OnEffect();
    }

}
