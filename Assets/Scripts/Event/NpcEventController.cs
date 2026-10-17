using System.Collections;
using Dialogue;
using InputSystem;
using Portal;
using Scripts_Creatures.Creatures;
using UnityEngine;

namespace Event
{
    public class NpcEventController : MonoBehaviour
    {
        private GameObject npc;
        private GameObject guardObj;
        private Guard guard;
        private GameObject player;
        private GameObject barricade;
        private bool departing, controlsHeld;
        private bool wasMoving, wasInteracting, wasInventory;
        [SerializeField] NpcDialogueData npcDialogueData;
        [SerializeField] string npcType;
        private bool isAlreadyTalk = false;
        private void Start()
        {
            guardObj = GameObject.Find("Guard");
            guard = guardObj != null ? guardObj.GetComponent<Guard>() : null;

            player = Character.Instance != null ? Character.Instance.gameObject : null;

            barricade = GameObject.Find("Barricade");

            npc = GameObject.Find("NPC");
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !isAlreadyTalk && !departing)
            {
                EventScheduler.Instance.eventObjects["FirstMeetNpcEventObject"].StopSound();
                NpcDialogueController.Instance.ShowDialogue(npcDialogueData.GetDialogues(npcType));
                isAlreadyTalk = true;
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && player != null && player.transform.position.x < -2 && !departing)
            {
                StartNpcEvent();
            }
        }

        private void StartNpcEvent()
        {
            if (departing) return;
            departing=true;
            var input=InputManager.Instance;
            wasMoving=input.GetKeyActive(ActionCode.MoveLeft);
            wasInteracting=input.GetKeyActive(ActionCode.Interaction);
            wasInventory=input.GetKeyActive(ActionCode.OpenInventory);
            controlsHeld=true;
            InputManager.Instance.SetMovementState(false);
            input.SetKeyActive(ActionCode.Interaction,false);
            input.SetKeyActive(ActionCode.OpenInventory,false);
            if (guard!=null) guard.StopPatrol();
            StartCoroutine(WaitAndFinishNpcEvent());
        }

        private IEnumerator WaitAndFinishNpcEvent()
        {
            try
            {
                var dialogue=NpcDialogueController.Instance;
                while (dialogue!=null && dialogue.IsDisplaying) yield return null;
                if (dialogue!=null)
                {
                    dialogue.ShowDialogue("먼저 출구를 열게요. 뒤따라오세요!");
                    while (dialogue!=null && dialogue.IsDisplaying) yield return null;
                }
                if (barricade!=null) barricade.SetActive(false);
                var door=GetComponent<Scripts_door.DoorObject>();
                if (door!=null) yield return door.OpenForPassage();
                if (npc!=null)
                {
                    var departure=npc.AddComponent<Lucy.Presentation.NurseDeparture>();
                    yield return departure.Run();
                    Destroy(npc);
                }
            }
            finally { FinishNpcEvent(); }
            Destroy(this);
        }

        private void FinishNpcEvent()
        {
            if (!controlsHeld) return;
            controlsHeld=false;
            if (InputManager.Instance!=null)
            {
                InputManager.Instance.SetMovementState(wasMoving);
                InputManager.Instance.SetKeyActive(ActionCode.Interaction,wasInteracting);
                InputManager.Instance.SetKeyActive(ActionCode.OpenInventory,wasInventory);
            }
            if (guard!=null) guard.StartPatrol();
        }
        private void OnDestroy() { FinishNpcEvent(); }
    }
}
