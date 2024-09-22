using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDialogue : InteractableObject
{
    [SerializeField] string npcType;
    [SerializeField] NpcDialogueData npcDialogueData;

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        NpcDialogueController.Instance.ShowDialogue(npcDialogueData.GetDialogues(npcType));
    }

    protected override void ActOnTrigger(Collider2D other)
    {
        NpcDialogueController.Instance.ShowDialogue(npcDialogueData.GetDialogues(npcType));
    }
}
