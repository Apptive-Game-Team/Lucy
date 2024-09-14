using System;
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

public class DoorObject : InteractableObject
{
    private ToggleableObjectSoundController soundController;

    [SerializeField] DoorInfo doorInfo;
    [SerializeField] GameObject door;

    protected override void ActOnTrigger(Collider2D other)
    {
        if (doorInfo.CheckCanOpen())
        {
            door.SetActive(!door.activeSelf);
            soundController.PlaySound(door.activeSelf);
            CreatureManager.Instance.UpdateMap();
        }
    }

    private void Awake()
    {
        soundController = GetComponent<ToggleableObjectSoundController>();
    }

    private void Start()
    {
        if(doorInfo.isLocked && doorInfo.keyId == ItemID.NONE) 
        {
            Debug.LogWarning("This Door is Locked, But Has not Key. Please Door");
        }
        soundController.SetType("Door");
    }
}
