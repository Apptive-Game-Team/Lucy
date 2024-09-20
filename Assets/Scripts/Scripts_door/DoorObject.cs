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
    private CreatureManager creatureManager;
    [SerializeField] DoorInfo doorInfo;
    [SerializeField] GameObject door;

    protected override void ActOnTrigger(Collider2D other)
    {
        if (doorInfo.CheckCanOpen())
        {
            door.SetActive(!door.activeSelf);
            soundController.PlaySound(door.activeSelf);
            creatureManager.UpdateMap();
        }
    }

    private void Awake()
    {
        soundController = GetComponent<ToggleableObjectSoundController>();
    }

    protected override void Start()
    {
        base.Start();
        creatureManager = ReferenceManager.Instance.FindComponentByName<CreatureManager>("CreatureManager");
        if(doorInfo.isLocked && doorInfo.keyId == ItemID.NONE)  
        {
            Debug.LogWarning("This Door is Locked, But Has not Key. Please Door");
        }
        soundController.SetType("Door");
    }
}
