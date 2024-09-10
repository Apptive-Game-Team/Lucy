using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SoundSystem;

public class SoundManager : SingletonObject<SoundManager>
{

    private void Awake()
    {
        base.Awake(); //Singleton

        if (audioSource == null)
        {
            try
            {
                audioSource = gameObject.GetComponent<AudioSource>();
            } catch
            {
                throw new Exception("AudioSource is not found");
            }
        }
    }

    [SerializeField]
    public SoundSources soundSources;

    [SerializeField]
    private AudioSource audioSource;

    private void Start()
    {
        PlayBackgroundMusic("HorrorStrange");
    }

    public void PlayBackgroundMusic(string name)
    {
        audioSource.clip = soundSources.GetByName(name).Value.sound;
        audioSource.Play();
    }

    public void StopBackgroundMusic(string name)
    {
        audioSource.Stop();
    }

}
