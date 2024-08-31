using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public GameObject TextObject;

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            TextObject.SetActive(true);
        }
    }
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player") && InputManager.Instance.GetKeyDown(ActionCode.Interaction))
        {
            ActOnTrigger(other);
        }
    }
    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            TextObject.SetActive(false);
        }
    }

    protected abstract void ActOnTrigger(Collider2D other);
}


