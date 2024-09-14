using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLight : MonoBehaviour
{
    Detector detector;

    List<Collider2D> targets = new List<Collider2D>();

    private void Awake()
    {
        detector = gameObject.GetComponent<Detector>();
    }

    void Start()
    {
        detector.SetViewAngle(360);
        detector.setLookingAngle(0);
        detector.SetTargetMask(LayerMask.GetMask("Player"));
        StartCoroutine(CheckLightUpdate());
    }

    IEnumerator CheckLightUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            targets.Clear();
            targets = detector.DetectByView();
            foreach (Collider2D target in targets)
            {
                if (target.CompareTag("Player"))
                {
                    target.gameObject.GetComponent<CharacterStat>().OnSpotLight();
                }
            }
        } 
    }
}
