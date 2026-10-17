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
    public int reduceAmount = 2;
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
        BindUI();
        if (PortalManager.Instance != null) PortalManager.Instance.SetSceneChangeListener(this);
        var soundTransform = transform.Find("PlayerStatusSoundController");
        audioSource = soundTransform != null ? soundTransform.GetComponent<AudioSource>() : null;
        if (audioSource != null && SoundManager.Instance != null && SoundManager.Instance.soundSources != null)
        {
            var heartbeat = SoundManager.Instance.soundSources.GetByName("Heartbeat");
            if (heartbeat.HasValue) audioSource.clip = heartbeat.Value.sound;
            audioSource.loop = true;
        }
        SetStats();
        UpdateStats();
        mentalCoroutine = StartCoroutine(ReduceMental());
    }

    private void OnDestroy() { if (instance == this) instance = null; }
    private void OnDisable()
    {
        isOnLight = false;
        onLightCounter = null;
        mentalCoroutine = null;
    }

    private void BindUI()
    {
        if (UIController.Instance == null) return;
        var canvas = UIController.Instance.transform.Find("StatCanvas 1");
        if (canvas == null) return;
        var mentalBar = canvas.Find("MentalBar");
        var staminaBar = canvas.Find("StaminaBar");
        if (mentalBar != null)
        {
            mentalSlider = mentalBar.GetComponent<Slider>();
            var counter = mentalBar.Find("Count_Mental");
            count_Mental = counter != null ? counter.GetComponent<TextMeshProUGUI>() : null;
        }
        if (staminaBar != null)
        {
            staminaSlider = staminaBar.GetComponent<Slider>();
            var counter = staminaBar.Find("Count_Stamina");
            count_Stamina = counter != null ? counter.GetComponent<TextMeshProUGUI>() : null;
        }
    }

    void ISceneChangeListener.OnSceneChange()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Puzzle_2F")
        {
            reduceAmount = 2;
            if (invertoryAndItem.FlashLight.instance != null) invertoryAndItem.FlashLight.instance.consumeAmount = 1;
        }
        BindUI();
        var camera = ReferenceManager.Instance != null ? ReferenceManager.Instance.FindComponentByName<CameraMove>("MainCamera") : null;
        hallucination = camera != null ? camera.hallucination : null;
        hallucinationSpriteRenderer = hallucination != null ? hallucination.GetComponent<SpriteRenderer>() : null;
        UpdateStats();
    }

    /// <summary>
    /// Resets mental and stamina to their maximum values.
    /// </summary>
    public void SetStats()
    {
        curMental = maxMental;
        curStamina = maxStamina;
        canRun = true;
        isRun = false;
    }
    
    /// <summary>
    /// Updates the UI elements to reflect current stat values.
    /// </summary>
    public void UpdateStats()
    {
        maxMental = Mathf.Max(1, maxMental);
        maxStamina = Mathf.Max(1, maxStamina);
        curMental = Mathf.Clamp(curMental, 0, maxMental);
        curStamina = Mathf.Clamp(curStamina, 0, maxStamina);
        if (mentalSlider != null) mentalSlider.value = curMental/maxMental;
        if (staminaSlider != null) staminaSlider.value = curStamina/maxStamina;
        if (count_Stamina != null) count_Stamina.text = string.Format("{0}/{1}", Mathf.FloorToInt(curStamina), maxStamina);
        if (count_Mental != null) count_Mental.text = string.Format("{0}/{1}", Mathf.FloorToInt(curMental), maxMental);
        float warning = Mathf.Clamp01(1 - curMental / (maxMental * MENTAL_WARNING_RATE));
        if (hallucinationSpriteRenderer != null)
        {
            Color color = hallucinationSpriteRenderer.color; color.a = warning / 3f;
            hallucinationSpriteRenderer.color = color; hallucination.SetActive(warning > 0);
        }
        if (audioSource != null)
        {
            audioSource.pitch = 1 + warning;
            if (warning > 0 && !audioSource.isPlaying) audioSource.Play();
            else if (warning == 0) audioSource.Stop();
        }
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
        if (curStamina >= Mathf.Min(STAMINA_RECOVERY_THRESHOLD, maxStamina))
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
        while (curMental > 0)
        {   
            yield return new WaitForSeconds(Mathf.Max(1f, delay));
            if (!isOnLight && !Lucy.Presentation.HospitalTutorial.ProtectLearning)
            {
                curMental = Mathf.Max(0, curMental - Mathf.Max(0, reduceAmount));
                UpdateStats();
                if (curMental <= 0 && Lucy.Presentation.GameSession.Instance != null) Lucy.Presentation.GameSession.Instance.Fail("빛을 잃고 어둠 속에서 길을 잃었습니다.");
            }
        }
    }
    
    /// <summary>
    /// Starts or restarts the mental reduction coroutine.
    /// Called when player is in darkness without a light source.
    /// </summary>
    public void StartMentalReduce()
    {
        if (!isActiveAndEnabled) { mentalCoroutine = null; return; }
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
