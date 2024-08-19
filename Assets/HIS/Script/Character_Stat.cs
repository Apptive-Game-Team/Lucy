using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character_Stat : MonoBehaviour
{
    public Slider mentalSlider;
    public Slider staminaSlider;

    [Header("Player Stat")]
    public float curMental;
    public float maxMental = 100;
    public float curStamina;
    public float maxStamina = 100;

    void Start()
    {
        SetStats();
    }

    void Update()
    {
        UpdateStats();
    }
    public void SetStats()
    {
        curMental = maxMental;
        curStamina = maxStamina;
    }
    
    public void UpdateStats()
    {
        mentalSlider.value = curMental/maxMental;
        staminaSlider.value = curStamina/maxStamina;
        if (mentalSlider.value <= 0)
        {
            mentalSlider.gameObject.SetActive(false);
        }
        else
        {
            mentalSlider.gameObject.SetActive(true);
        }
        if(staminaSlider.value <= 0)
        {
            staminaSlider.gameObject.SetActive(false);
        }
        else
        {
            staminaSlider.gameObject.SetActive(true);
        }
    }
}
