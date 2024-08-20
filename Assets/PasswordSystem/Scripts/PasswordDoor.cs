using System.Collections;
using System.Collections.Generic;
using Dialogue;
using UnityEngine;

public class PasswordDoor : PasswordObject
{
    public GameObject Text;
    public DialogueController2 dialogueController2;

    [SerializeField] DoorInfo doorInfo;
    [SerializeField] GameObject door;

    public override void Unlock()
    {
        base.Unlock();
        doorInfo.isLocked = false;
        door.SetActive(!door.activeSelf);
    }

    //public void DialogueControll()
    //{
    //    dialogueController2.SetDialogueFlag(doorInfo.DialogueId);
    //}

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
            if (Input.GetKey(KeyCode.Z))
            {
                print("tqlf");
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
