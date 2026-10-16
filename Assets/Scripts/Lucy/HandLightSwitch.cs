using invertoryAndItem;
using UnityEngine;

namespace Lucy
{
    public class HandLightSwitch : MonoBehaviour
    {
        [SerializeField] public GameObject handlightObject;
        public static HandLightSwitch instance;
        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(this); return; }
            instance = this;
        }
        private void OnDestroy() { if (instance == this) instance = null; }
        public void TurnOnHandLight()
        {
            if (handlightObject != null)
                handlightObject.SetActive(FlashLight.instance != null && FlashLight.instance.battery > 0);
        }
        public void TurnOffHandLight()
        {
            if (handlightObject != null) handlightObject.SetActive(false);
        }
    }
}
