using System.Collections;
using System.Collections.Generic;
using CharacterCamera;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStat : MonoBehaviour, ISceneChangeListener
{
    public static CharacterStat instance;

    public Slider mentalSlider;
    public Slider staminaSlider;
    public TextMeshProUGUI count_Stamina;
    public TextMeshProUGUI count_Mental;
    public float delay;

    private Coroutine mentalCoroutine;
    private Coroutine staminaCoroutine;
    [Header("Player Stat")]
    public float curMental;
    public float maxMental = 100;
    public float curStamina;
    public float maxStamina = 100;
    public int reduceAmount = 10;
    private GameObject hallucination;
    private SpriteRenderer hallucinationSpriteRenderer;

    private AudioSource audioSource;
    private const float MENTAL_WARNING_RATE = 0.5f;

    [SerializeField] private bool isOnLight = false;

    [SerializeField] public bool isRun = false;
    [SerializeField] public bool canRun = true;
    private Coroutine onLightCounter;

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
        PortalManager.Instance.SetSceneChangeListener(this);
        audioSource = transform.Find("PlayerStatusSoundController").GetComponent<AudioSource>();
        audioSource.clip = SoundManager.Instance.soundSources.GetByName("Heartbeat").Value.sound;
        SetStats();
        UpdateStats();
        mentalCoroutine = StartCoroutine(ReduceMental());
    }
    void ISceneChangeListener.OnSceneChange()
    {
        hallucination = ReferenceManager.Instance.FindComponentByName<CameraMove>("MainCamera").hallucination;
        hallucinationSpriteRenderer = hallucination.GetComponent<SpriteRenderer>();
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
        count_Stamina.text = string.Format("{0}/{1}", Mathf.FloorToInt(curStamina), maxStamina);
        count_Mental.text = string.Format("{0}/{1}", Mathf.FloorToInt(curMental), maxMental);
        /*if (mentalSlider.value <= 0)
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
        }*/
    }
    public void ChangeStamina(int n)
    {
        curStamina += n * Time.deltaTime;
        if (curStamina > maxStamina)
        {
            curStamina = maxStamina;
        }
        if (curStamina <= 0)
        {
            curStamina = 0;
            canRun = false;
        }
        if (curStamina >= 50)
        {
            canRun = true;
        }
        UpdateStats();
    }

    public IEnumerator ReduceMental()
    {
        yield return new WaitUntil(() => hallucinationSpriteRenderer != null);

        while (curMental > 0)
        {   
            yield return new WaitForSecondsRealtime(delay);
            if (!isOnLight)
            {
                curMental -= reduceAmount;
                audioSource.pitch = 1 + (float)(((maxMental * MENTAL_WARNING_RATE) - curMental) / (maxMental * MENTAL_WARNING_RATE));
                float alpha = (float)(((maxMental * MENTAL_WARNING_RATE) - curMental) / (maxMental * MENTAL_WARNING_RATE)) / 3;

                Color hallucinationColor = hallucinationSpriteRenderer.color;
                hallucinationColor.a = alpha;
                hallucinationSpriteRenderer.color = hallucinationColor;

                if (curMental <= maxMental * MENTAL_WARNING_RATE && !audioSource.isPlaying)
                {
                    audioSource.Play();
                    hallucination.SetActive(true);
                } else  if (curMental > maxMental * MENTAL_WARNING_RATE)
                {
                    audioSource.Stop();
                    hallucination.SetActive(false);
                }
                UpdateStats();
            }
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
    public void OnSpotLight()
    {
        isOnLight = true;
        if (onLightCounter == null)
        {
            onLightCounter = StartCoroutine(OnLightCounter());
        } else
        {
            StopCoroutine(onLightCounter);
            onLightCounter = StartCoroutine(OnLightCounter());
        }
        
    }

    IEnumerator OnLightCounter()
    {
        yield return new WaitForSeconds(2);
        isOnLight = false;
        onLightCounter = null;
    }
}
