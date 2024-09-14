using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SoundSystem;

public enum UISound{
    BUTTON_CLICK=0,
}

public class UISoundController : SoundController, IKeyInputListener
{
    private List<SoundSource> soundSources = new List<SoundSource>();

    private void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitSoundSources();
        InputManager.Instance.SetKeyListener(this);
    }

    public void PlayButton(UISound uISound)
    {
        audioSource.clip = soundSources[(int)uISound].sound;
        audioSource.Play();
    }

    private void InitSoundSources()
    {
        soundSources.Add(SoundManager.Instance.soundSources.GetByName("ButtonSound").Value);
    }

    void IKeyInputListener.OnKeyDown(ActionCode action)
    {
        if (action == ActionCode.SelectClick || action == ActionCode.OpenInventory)
        {
            PlayButton(UISound.BUTTON_CLICK);
        }
    }
}
