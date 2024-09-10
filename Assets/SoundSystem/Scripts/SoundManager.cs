using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SoundSystem;

public class SoundManager : SingletonObject
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
    private SoundSources soundSources;

    [SerializeField]
    private AudioSource audioSource;


    
}
