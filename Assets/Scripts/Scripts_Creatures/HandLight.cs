using System.Collections.Generic;
using Lucy;
using Scripts_Creatures.Creatures;
using Scripts_Creatures.Util;
using UnityEngine;

namespace Scripts_Creatures
{
    public class HandLight : MonoBehaviour
    {
        Detector detector;

        HandLightSwitch handLightSwitch;

        void Start()
        {
            handLightSwitch = GetComponent<HandLightSwitch>();
            detector = gameObject.GetComponent<Detector>();
            detector.SetTargetMask(LayerMask.GetMask("Creature"));
        }

        void Update()
        {
            if (handLightSwitch != null && detector != null && handLightSwitch.handlightObject != null &&
                handLightSwitch.handlightObject.activeInHierarchy)
            {
                List<Collider2D> targets = detector.DetectByView();
                foreach (Collider2D target in targets)
                {
                    // Only light-sensitive subtypes react; guards and ordinary monsters keep chasing.
                    var avoider = target.GetComponentInParent<Avoider>();
                    if (avoider != null)
                        avoider.OnDetectedByHandLight(transform.position);
                }
            }
        }
    }
}

