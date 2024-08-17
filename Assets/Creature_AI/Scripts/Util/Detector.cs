using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] bool debugMode = true;
    [Range(0f, 360f)]
    [SerializeField] float viewAngle = 120f;
    float viewDistance = 5f;

    [Range(0f, 360f)]
    [SerializeField] float lookingAngle = 3f ;
    [SerializeField] LayerMask targetMask;
    LayerMask obstacleMask;
    List<Collider2D> hitTargetList = new List<Collider2D>();

    Vector3? targetPosition;

    public float getLookingAngle()
    {
        return lookingAngle;
    }


    public void setLookingAngle(float angle)
    {
        lookingAngle = angle;
    }

    public void SetTargetMask(LayerMask layerMask)
    {
        targetMask = layerMask;
    }


    public List<Collider2D> DetectByView()
    {
        hitTargetList.Clear();
        Collider2D[] targets = FindNearColliders();
        
        foreach (Collider2D collider in targets)
        {
            targetPosition = null;
            Vector3 colliderPosition = collider.transform.position;
            Vector3 targetDirection = (colliderPosition - transform.position).normalized;
            float targetAngle = Mathf.Acos(Vector3.Dot(
                    AngleToDir(lookingAngle),
                    targetDirection
                )) * Mathf.Rad2Deg;

            if (targetAngle <= viewAngle * 0.5f &&
                !Physics2D.Raycast(
                    transform.position,
                    targetDirection,
                    Vector3.Distance(transform.position, colliderPosition),
                    obstacleMask
                ))
            {

                targetPosition = colliderPosition;
                hitTargetList.Add(collider);
            }
        }
        return hitTargetList;
    }

    private void Start()
    {
        obstacleMask = LayerMask.GetMask("Wall");
    }


    Vector3 AngleToDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), Mathf.Cos(radian));
    }

    float DirToAngle(Vector3 direction)
    {
        direction.Normalize();
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        return angle;
    }


    public void SetLookingDirection(Vector3 direction)
    {
        lookingAngle = DirToAngle(direction);
    }

    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Gizmos.color = Color.red;
            Vector3 myPos = transform.position;
            Gizmos.DrawWireSphere(myPos, viewDistance);

            Vector3 rightDir = AngleToDir(lookingAngle + viewAngle * 0.5f);
            Vector3 leftDir = AngleToDir(lookingAngle - viewAngle * 0.5f);
            Vector3 lookDir = AngleToDir(lookingAngle);

            if (targetPosition.HasValue)
            {
                Debug.DrawLine(
                 transform.position,
                 targetPosition.Value,
                 Color.red
                 );
            }



            Debug.DrawRay(myPos, rightDir * viewDistance, Color.blue);
            Debug.DrawRay(myPos, leftDir * viewDistance, Color.blue);
            Debug.DrawRay(myPos, lookDir * viewDistance, Color.cyan);
        }

    }

    private Collider2D[] FindNearColliders()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(
                transform.position,
                viewDistance,
                targetMask
            );

        return targets;
    }
}
