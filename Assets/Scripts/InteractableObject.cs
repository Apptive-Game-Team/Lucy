using InputSystem;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour, IKeyInputListener
{
    public GameObject TextObject;
    private Collider2D other;
    protected virtual void Start()
    {
        if (TextObject != null)
        {
            TextObject.SetActive(false);
        }
        InputManager.Instance.SetKeyListener(this);
    }

    protected virtual void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.RemoveKeyListener(this);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (TextObject != null)
            {
                TextObject.SetActive(true);
            }
            this.other = other;
        }
    }

    void IKeyInputListener.OnKeyDown(ActionCode action)
    {
        if (action == ActionCode.Interaction && other != null)
        //if (other.gameObject.tag.Equals("Player"))
        {
            ActOnTrigger(other);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (TextObject != null)
            {
                TextObject.SetActive(false);
            }
            this.other = null;
        }
    }

    protected abstract void ActOnTrigger(Collider2D other);
}


