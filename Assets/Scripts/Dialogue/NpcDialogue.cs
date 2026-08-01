using UnityEngine;

namespace Dialogue
{
    public class NpcDialogue : InteractableObject
    {
        [SerializeField] string npcType;
        [SerializeField] NpcDialogueData npcDialogueData;

        // OnTriggerEnter2D is left to the base class: it shows the prompt and stores the
        // collider ActOnTrigger needs. Overriding it skipped both and fired the dialogue
        // on walk-in instead of on the interaction key.
        protected override void ActOnTrigger(Collider2D other)
        {
            NpcDialogueController.Instance.ShowDialogue(npcDialogueData.GetDialogues(npcType));
        }
    }
}
