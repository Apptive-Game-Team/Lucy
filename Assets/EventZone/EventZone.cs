using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventZoneID
{
    NONE,
    EVENT_PICK_HANDLIGHT,
}

public class EventZone : MonoBehaviour
{

    [SerializeField] EventZoneID eventZoneID;

    private void OnTriggerStay2D(Collider2D collision)
    {
        
    }
}
