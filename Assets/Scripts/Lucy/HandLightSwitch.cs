using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLightSwitch : MonoBehaviour
{
    [SerializeField] public GameObject handlightObject;
    public static HandLightSwitch instance;

    private void Awake()
    {
        instance = this;
    }
    public void TurnOnHandLight()
    {
        if ( FlashLight.instance.battery>=0)
        {
            handlightObject.SetActive(true);
        }
    }

    public void TurnOffHandLight()
    {
        handlightObject.SetActive(false);
    }
}
