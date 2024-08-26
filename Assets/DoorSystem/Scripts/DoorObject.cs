using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using Dialogue;
using UnityEngine;

[System.Serializable]
public class DoorInfo
{
    public ItemID keyId;
    public int DialogueId;
    public bool isLocked;
    public bool isOpened;

    public bool CheckCanOpen()
    {
        return !isLocked || Array.Exists(Inventory.instance.slots,itemSlot => itemSlot.item.itemId == keyId);
    }
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
            if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
            {
                if(doorInfo.CheckCanOpen())
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

    private void Start()
    {
        if(doorInfo.isLocked && doorInfo.keyId == ItemID.NONE) 
        {
            Debug.LogWarning("This Door is Locked, But Has not Key. Please Door");
        }
    }
}
