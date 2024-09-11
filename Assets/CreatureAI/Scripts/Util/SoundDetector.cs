using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SoundDetector : MonoBehaviour
{
    private float maxDistance = 30f;

    [SerializeField] bool debugMode = true;

    [SerializeField] LayerMask targetMask;

    LayerMask obstacleMask;

    List<Collider2D> hitTargetList = new List<Collider2D>();

    Vector3? targetPosition;

    public void SetTargetMask(LayerMask targetMask)
    {
        this.targetMask = targetMask;
    }

    public List<Collider2D> Detect()
    {
        Collider2D[] targets = FindNearColliders();
        hitTargetList.Clear();

        foreach(Collider2D target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            AudioSource source = target.GetComponent<AudioSource>();
            if (distance > target.GetComponent<AudioSource>().maxDistance && source.isPlaying)
            {
                hitTargetList.Add(target);
            }
        }
        if (debugMode)
        {
            print("sound detector found " + hitTargetList.Count + " target");
        }
        

        return hitTargetList;
    }

    private Collider2D[] FindNearColliders()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(
                transform.position,
                maxDistance,
                targetMask
            );

        return targets;
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = Color.red;
            Vector3 myPos = transform.position;
            Gizmos.DrawWireSphere(myPos, maxDistance);

            if (targetPosition.HasValue)
            {
                Debug.DrawLine(
                 transform.position,
                 targetPosition.Value,
                 Color.red
                 );
            }
        }
    }
}
