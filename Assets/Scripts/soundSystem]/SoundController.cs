using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundController : MonoBehaviour
{
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            throw new Exception("Audio Source is not found");
        }
    }
}
