using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashLight : MonoBehaviour
{
    public static FlashLight instance;
    public int consumeAmount = 1;
    public int battery = 1;
    public float delay;
    public ItemData itemData;
    public Coroutine batteryCoroutine;

    [Header("Activated Battery")]
    public Image[] activeBatteries;

    [Header("UnActivated Battery")]
    public Image[] inactiveBatteries;

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
        TurnOffUi();
    }

    void Update()
    {

    }

    public void SetUi()
    {
        for (int i = 0; i < activeBatteries.Length; i++)
        {
            if (i < battery)
            {
                activeBatteries[i].gameObject.SetActive(true);
                inactiveBatteries[i].gameObject.SetActive(false);
            }
            else
            {
                activeBatteries[i].gameObject.SetActive(false);
                inactiveBatteries[i].gameObject.SetActive(true);
            }
        }
    }

    public void UpdateUi()
    {
        for (int i = 0; i < activeBatteries.Length; i++)
        {
            if (i < battery)
            {
                activeBatteries[i].gameObject.SetActive(true);
                inactiveBatteries[i].gameObject.SetActive(false);
            }
            else
            {
                activeBatteries[i].gameObject.SetActive(false);
                inactiveBatteries[i].gameObject.SetActive(true);
            }
        }

        if (battery <= 0)
        {

        }
    }

    public void TurnOffUi()
    {
        foreach (var activeBattery in activeBatteries)
        {
            activeBattery.gameObject.SetActive(false);
        }
        foreach (var inactiveBattery in inactiveBatteries)
        {
            inactiveBattery.gameObject.SetActive(false);
        }
    }

    public IEnumerator ConsumeBattery()
    {
        while (battery > 0)
        {
            yield return new WaitForSecondsRealtime(delay);
            battery -= consumeAmount;
            UpdateUi();
        }
        if (battery <= 0)
        {
            battery = 0;
            StopConsumeBattery();
            CharacterStat.instance.StartMentalReduce();
            HandLightSwitch.instance.TurnOffHandLight();
        }
    }
    public void StartConsumeBattery()
    {
        if (batteryCoroutine != null)
        {
            StopCoroutine(batteryCoroutine);
        }
        batteryCoroutine = StartCoroutine(ConsumeBattery());
    }

    public void StopConsumeBattery()
    {
        if (batteryCoroutine != null)
        {
            StopCoroutine(batteryCoroutine);
            batteryCoroutine = null;
        }
    }
}