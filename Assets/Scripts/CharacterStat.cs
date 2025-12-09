using System.Collections;
using Lucy;
using Portal;
using ReferenceSystem;
using soundSystem_;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's mental and stamina stats.
/// Mental decreases in darkness and triggers hallucination effects at low levels.
/// Stamina is consumed when running and recovers when walking.
/// </summary>
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
    private const float STAMINA_RECOVERY_THRESHOLD = 50f;
    private const float SPOTLIGHT_EFFECT_DURATION = 2f;

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
        Transform canvas = UIController.Instance.transform.Find("StatCanvas 1");
        Transform mentalBar = canvas.Find("MentalBar");
        Transform staminaBar = canvas.Find("StaminaBar");
        mentalSlider = mentalBar.GetComponent<Slider>();
        staminaSlider = staminaBar.GetComponent<Slider>();
        count_Mental = mentalBar.Find("Count_Mental").GetComponent<TextMeshProUGUI>();
        count_Stamina = staminaBar.Find("Count_Stamina").GetComponent<TextMeshProUGUI>();
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

    /// <summary>
    /// Resets mental and stamina to their maximum values.
    /// </summary>
    public void SetStats()
    {
        curMental = maxMental;
        curStamina = maxStamina;
    }
    
    /// <summary>
    /// Updates the UI elements to reflect current stat values.
    /// </summary>
    public void UpdateStats()
    {
        mentalSlider.value = curMental/maxMental;
        staminaSlider.value = curStamina/maxStamina;
        count_Stamina.text = string.Format("{0}/{1}", Mathf.FloorToInt(curStamina), maxStamina);
        count_Mental.text = string.Format("{0}/{1}", Mathf.FloorToInt(curMental), maxMental);
    }
    
    /// <summary>
    /// Changes stamina by the specified rate per frame.
    /// Disables running when stamina is depleted and re-enables at threshold.
    /// </summary>
    /// <param name="n">Rate of change per second (negative to drain, positive to recover)</param>
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
        if (curStamina >= STAMINA_RECOVERY_THRESHOLD)
        {
            canRun = true;
        }
        UpdateStats();
    }

    /// <summary>
    /// Coroutine that continuously reduces mental stat when player is in darkness.
    /// Triggers hallucination effects (visual overlay and heartbeat audio) when mental drops below warning threshold.
    /// </summary>
    public IEnumerator ReduceMental()
    {
        yield return new WaitUntil(() => hallucinationSpriteRenderer != null);

        while (curMental > 0)
        {   
            yield return new WaitForSecondsRealtime(delay);
            if (!isOnLight)
            {
                curMental -= reduceAmount;
                // Increase heartbeat pitch as mental decreases
                audioSource.pitch = 1 + (float)(((maxMental * MENTAL_WARNING_RATE) - curMental) / (maxMental * MENTAL_WARNING_RATE));
                // Gradually increase hallucination overlay opacity
                float alpha = (float)(((maxMental * MENTAL_WARNING_RATE) - curMental) / (maxMental * MENTAL_WARNING_RATE)) / 3;
                yield return new WaitUntil(() => hallucinationSpriteRenderer != null);
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
    
    /// <summary>
    /// Starts or restarts the mental reduction coroutine.
    /// Called when player is in darkness without a light source.
    /// </summary>
    public void StartMentalReduce()
    {
        if (mentalCoroutine != null)
        {
            StopCoroutine(mentalCoroutine);
        }
        mentalCoroutine = StartCoroutine(ReduceMental());
    }

    /// <summary>
    /// Stops the mental reduction coroutine.
    /// Called when player equips a flashlight or finds another light source.
    /// </summary>
    public void StopMentalReduce()
    {
        if (mentalCoroutine != null)
        {
            StopCoroutine(mentalCoroutine);
            mentalCoroutine = null;
        }
    }
    
    /// <summary>
    /// Temporarily pauses mental reduction when player enters a spotlight.
    /// Effect lasts for SPOTLIGHT_EFFECT_DURATION seconds.
    /// </summary>
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
        yield return new WaitForSeconds(SPOTLIGHT_EFFECT_DURATION);
        isOnLight = false;
        onLightCounter = null;
    }
}
