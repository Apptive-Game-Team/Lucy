using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLightSwitch : MonoBehaviour
{
    [SerializeField] GameObject handlightObject;
    public static HandLightSwitch instance;

    private void Awake()
    {
        instance = this;
    }
    public void TurnOnHandLight()
    {
        handlightObject.SetActive(true);
    }

    public void TurnOffHandLight()
    {
        handlightObject.SetActive(false);
    }
}
