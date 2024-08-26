using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class Character_Stat : MonoBehaviour
{
    public static Character_Stat instance;

    public Slider mentalSlider;
    public Slider staminaSlider;
    public TextMeshProUGUI Count_Stamina;
    public TextMeshProUGUI Count_Mental;
    public float delay;

    private Coroutine mentalCoroutine;

    [Header("Player Stat")]
    public float curMental;
    public float maxMental = 100;
    public float curStamina;
    public float maxStamina = 100;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetStats();
        UpdateStats();
        mentalCoroutine = StartCoroutine(ReduceMental());
    }

    void Update()
    {

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
        Count_Stamina.text = (curStamina.ToString() + "/" + maxStamina.ToString());
        Count_Mental.text = (curMental.ToString() + "/" + maxMental.ToString());
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

    public IEnumerator ReduceMental()
    {
        while (curMental > 0)
        {   
            yield return new WaitForSecondsRealtime(delay);
            curMental-=10;
            UpdateStats();
        }
    }
    public void StartMentalReduce()
    {
        if (mentalCoroutine != null)
        {
            StopCoroutine(mentalCoroutine);
        }
        mentalCoroutine = StartCoroutine(ReduceMental());
    }

    public void StopMentalReduce()
    {
        if (mentalCoroutine != null)
        {
            StopCoroutine(mentalCoroutine);
            mentalCoroutine = null;
        }
    }
}
