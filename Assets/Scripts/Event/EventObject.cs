using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventObject : MonoBehaviour
{
    private EventSoundController eventSoundController;

    void Start()
    {
        eventSoundController = GetComponent<EventSoundController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
