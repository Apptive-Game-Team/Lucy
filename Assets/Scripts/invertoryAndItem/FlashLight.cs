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
        [Header("Activated Battery")] public Image[] activeBatteries;
        [Header("UnActivated Battery")] public Image[] inactiveBatteries;
        private bool uiVisible;
        public int Capacity => activeBatteries != null && activeBatteries.Length > 0 ? activeBatteries.Length : 9;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(this); return; }
            instance = this;
            battery = Mathf.Clamp(battery, 0, Capacity);
        }

        private void OnDestroy() { if (instance == this) instance = null; }
        private void Start() { UpdateUi(); }
        public void SetUi() { uiVisible = true; UpdateUi(); }
        public void TurnOffUi() { uiVisible = false; UpdateUi(); }

        public void UpdateUi()
        {
            if (activeBatteries != null)
                for (int i = 0; i < activeBatteries.Length; i++)
                    if (activeBatteries[i] != null) activeBatteries[i].gameObject.SetActive(uiVisible && i < battery);
            if (inactiveBatteries != null)
                for (int i = 0; i < inactiveBatteries.Length; i++)
                    if (inactiveBatteries[i] != null) inactiveBatteries[i].gameObject.SetActive(uiVisible && i >= battery);
        }

        public bool Refill(int amount)
        {
            if (amount <= 0 || battery >= Capacity) return false;
            bool wasEmpty = battery <= 0;
            battery = Mathf.Clamp(battery + amount, 0, Capacity);
            if (wasEmpty && Inventory.instance != null && Inventory.instance.IsItemEquipped(itemData))
            {
                if (HandLightSwitch.instance != null) HandLightSwitch.instance.TurnOnHandLight();
                if (CharacterStat.instance != null) CharacterStat.instance.StopMentalReduce();
                StartConsumeBattery();
            }
            UpdateUi();
            return true;
        }

        public IEnumerator ConsumeBattery()
        {
            while (battery > 0)
            {
                yield return new WaitForSeconds(Mathf.Max(1f, delay));
                battery = Mathf.Max(0, battery - Mathf.Max(1, consumeAmount));
                UpdateUi();
            }
            batteryCoroutine = null;
            if (CharacterStat.instance != null) CharacterStat.instance.StartMentalReduce();
            if (HandLightSwitch.instance != null) HandLightSwitch.instance.TurnOffHandLight();
        }

        public void StartConsumeBattery()
        {
            StopConsumeBattery();
            if (battery > 0) batteryCoroutine = StartCoroutine(ConsumeBattery());
            else
            {
                if (CharacterStat.instance != null) CharacterStat.instance.StartMentalReduce();
                if (HandLightSwitch.instance != null) HandLightSwitch.instance.TurnOffHandLight();
            }
        }

        public void StopConsumeBattery()
        {
            if (batteryCoroutine == null) return;
            StopCoroutine(batteryCoroutine);
            batteryCoroutine = null;
        }
    }
}
