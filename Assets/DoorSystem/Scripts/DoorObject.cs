using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dialogue;
using UnityEngine;

[System.Serializable]
public class DoorInfo
{
    public int keyId;
    public int DialogueId;
    public bool isLocked;
    public bool isOpened;
}

public class DoorObject : MonoBehaviour
{
    public GameObject Text;
    public DialogueController2 dialogueController2; 
    [SerializeField] DoorInfo doorInfo;
    [SerializeField] GameObject door;

    public void DialogueControll()
    {
        dialogueController2.SetDialogueFlag(doorInfo.DialogueId);
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
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if(!doorInfo.isLocked)
                {   
                    DialogueControll();
                    door.SetActive(!door.activeSelf);
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
