using UnityEngine;

public abstract class InteractableObject : MonoBehaviour, IKeyInputListener
{
    public GameObject TextObject;
    private Collider2D other;
    void Start()
    {
        TextObject.SetActive(false);
        InputManager.Instance.SetKeyListener(this);

    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        print(gameObject.name + "");
        if (other.gameObject.tag.Equals("Player"))
        {
            TextObject.SetActive(true);
            this.other = other;
        }
    }

    void IKeyInputListener.OnKeyDown(ActionCode action)
    {
        print("asdf");
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
            TextObject.SetActive(false);
            this.other = null;  
        }
    }

    protected abstract void ActOnTrigger(Collider2D other);
}


