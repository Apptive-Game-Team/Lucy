using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour, IKeyInputListener
{
    public GameObject TextObject;
    private bool isTriggerStay = false;
    private Collider2D other;

    void Start(){
        InputManager.Instance.SetKeyListener(this);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            isTriggerStay = true;
            this.other = other;
            TextObject.SetActive(true);
        }
    }

    // void IKeyInputListener.OnKeyDown(ActionCode action){
    //     if (isTriggerStay && action == ActionCode.Interaction){
    //         ActOnTrigger(other);
    //     }
    // }
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
            isTriggerStay = false;
            other= null;
            TextObject.SetActive(false);
        }
    }

    protected abstract void ActOnTrigger(Collider2D other);
}


