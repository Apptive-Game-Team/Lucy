using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSoundController : SoundController
{
    protected override void Awake()
    {
        base.Awake();
    }

    public void PlaySound(AudioClip audioClip, bool isLoop)
    {
        audioSource.clip = audioClip;
        audioSource.loop = isLoop;
        audioSource.Play();
    }
}
