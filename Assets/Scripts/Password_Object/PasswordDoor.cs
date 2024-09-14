using UnityEngine;

public class PasswordDoor : PasswordObject
{
    public GameObject Text;

    [SerializeField] DoorInfo doorInfo;
    [SerializeField] GameObject door;

    public override void Unlock()
    {
        base.Unlock();
        doorInfo.isLocked = false;
        door.SetActive(!door.activeSelf);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(true);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
            {
                if (!doorInfo.isLocked)
                {
                    door.SetActive(!door.activeSelf);
                } else 
                {
                    OpenPasswordPage();
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(false);
        }
    }
}
