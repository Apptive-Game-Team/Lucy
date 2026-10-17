using InputSystem;
using UnityEngine;
using System.Collections.Generic;

public abstract class InteractableObject : MonoBehaviour, IKeyInputListener
{
    public GameObject TextObject;
    private Collider2D other;
    public static readonly HashSet<InteractableObject> Nearby = new HashSet<InteractableObject>();
    public bool CanInteract => isActiveAndEnabled && other != null && other.gameObject.activeInHierarchy;
    protected virtual void Start()
    {
        if (TextObject != null) TextObject.SetActive(false);
        InputManager.Instance.SetKeyListener(this);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (TextObject != null) TextObject.SetActive(false);
            this.other = other;
            Nearby.Add(this);
        }
    }

    void IKeyInputListener.OnKeyDown(ActionCode action)
    {
        if (action == ActionCode.Interaction && CanInteract)
        //if (other.gameObject.tag.Equals("Player"))
        {
            ActOnTrigger(other);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (TextObject != null) TextObject.SetActive(false);
            this.other = null;  
            Nearby.Remove(this);
        }
    }

    protected abstract void ActOnTrigger(Collider2D other);

    protected virtual void OnDisable()
    {
        Nearby.Remove(this);
        other = null;
        if (TextObject != null) TextObject.SetActive(false);
    }
}


