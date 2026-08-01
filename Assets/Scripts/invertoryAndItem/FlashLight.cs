using System.Collections;
using Lucy;
using ScriptableObjects.ScriptableObject_items.Script;
using UnityEngine;
using UnityEngine.UI;

namespace invertoryAndItem
{
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

        public void SetUi()
        {
            UpdateUi();
        }

        public void UpdateUi()
        {
            int count = Mathf.Min(activeBatteries.Length, inactiveBatteries.Length);
            for (int i = 0; i < count; i++)
            {
                bool isCharged = i < battery;
                activeBatteries[i].gameObject.SetActive(isCharged);
                inactiveBatteries[i].gameObject.SetActive(!isCharged);
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
                UpdateUi();
                // This coroutine is finishing anyway - calling StopConsumeBattery() here
                // stopped itself mid-way, so just drop the handle.
                batteryCoroutine = null;
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
}