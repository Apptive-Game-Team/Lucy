using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class HandLight : MonoBehaviour
    {
        Detector detector;
        Avoider avoider;

        HandLightSwitch handLightSwitch;

        void Start()
        {
            handLightSwitch = GetComponent<HandLightSwitch>();
            detector = gameObject.GetComponent<Detector>();
            detector.SetTargetMask(LayerMask.GetMask("Creature"));
        }

        void Update()
        {
            if (handLightSwitch.handlightObject.activeSelf)
            {
                List<Collider2D> targets = detector.DetectByView();
                foreach (Collider2D target in targets)
                {
                    try
                    {
                        avoider = target.GetComponent<Avoider>();
                        avoider.OnDetectedByHandLight(transform.position);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
    }
}

