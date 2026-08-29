using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Portal
{
    public class CameraEffector : SingletonObject<CameraEffector>
    {
        Image fadeImage;
        const float FADE_DURATION = 2f;

        private void Start()
        {
            fadeImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        }

        // unscaledDeltaTime: dialogues and puzzles set timeScale to 0, which used to make
        // a fade never finish and hang the scene transition on a black screen.
        public IEnumerator FadeOut()
        {
            float curTime = 0;

            while (curTime < FADE_DURATION)
            {
                yield return null;
                curTime += Time.unscaledDeltaTime;
                SetFadeAlpha(curTime / FADE_DURATION);
            }
        }
        public IEnumerator FadeIn()
        {
            float curTime = FADE_DURATION;

            while (curTime > 0)
            {
                yield return null;
                curTime -= Time.unscaledDeltaTime;
                SetFadeAlpha(curTime / FADE_DURATION);
            }
        }

        private void SetFadeAlpha(float alpha)
        {
            if (fadeImage == null)
            {
                return;
            }
            fadeImage.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
        }

        public IEnumerator FadeOutIn()
        {
            yield return FadeOut();
            yield return FadeIn();
        }

    }
}