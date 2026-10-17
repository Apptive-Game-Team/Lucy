using System.Collections;
using System.Collections.Generic;
using Portal;
using ReferenceSystem;
using TMPro;
using UnityEngine;

namespace Dialogue
{
    public enum NpcType
    {
        PUZZLENPC,
    }

    public abstract class NpcDialogues
    {
        public abstract void Show();
    }

    public class puzzleNpc : NpcDialogues
    {
        private readonly NpcDialogueData npcDialogueData;
        public puzzleNpc(NpcDialogueData npcDialogueData)
        {
            this.npcDialogueData = npcDialogueData;
        }

        public override void Show()
        {
            NpcDialogueController.Instance.ShowDialogue(npcDialogueData.GetDialogues(NpcType.PUZZLENPC.ToString()));
        }
    }

    public class NpcDialogueController : SingletonObject<NpcDialogueController>, ISceneChangeListener
    {
        public NpcDialogueData npcDialogueData;
        protected List<NpcDialogues> npcDialogue;

        private ReferableObject dialogueImages;
        public GameObject dialogueImage;
        public GameObject dialogueCharacter;
        public TMP_Text dialogueText;

        public float typingSpeed = 0.05f;
        public float delayBetweenDialogues = 1.2f;
        private bool displaying;
        public bool IsDisplaying => displaying;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            PortalManager.Instance.SetSceneChangeListener(this);
        }

        void ISceneChangeListener.OnSceneChange()
        {
            StopAllCoroutines();
            displaying = false;
            dialogueImage = dialogueCharacter = null;
            dialogueText = null;
            dialogueImages =  ReferenceManager.Instance.FindComponentByName<ReferableObject>("DialogueImages");
            if (dialogueImages == null) return; // Maze and ending have no NPC dialogue UI.
            var background = dialogueImages.transform.Find("DialogueBackground");
            var portrait = dialogueImages.transform.Find("LucyDialogueImage");
            dialogueImage = background != null ? background.gameObject : null;
            dialogueCharacter = portrait != null ? portrait.gameObject : null;
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "0201" && portrait != null)
            {
                var image = portrait.GetComponent<UnityEngine.UI.Image>();
                var nurse = Resources.Load<Sprite>("Presentation/Nurse");
                if (image != null && nurse != null) { image.sprite = nurse; image.preserveAspect = true; }
            }
            dialogueText = dialogueImages.GetComponentInChildren<TMP_Text>(true);
            if (dialogueImage != null) dialogueImage.SetActive(false);
            if (dialogueCharacter != null) dialogueCharacter.SetActive(false);

            npcDialogue = new List<NpcDialogues>()
            {
                new puzzleNpc(npcDialogueData)
            };
        }

        public void ShowDialogue(string[] dialogues, float delayBetweenDialogues = 1.5f) // 두 문장 이상 출력
        {
            if (displaying || dialogues == null || dialogueImage == null || dialogueCharacter == null || dialogueText == null) return;
            StartCoroutine(DisplayDialogues(dialogues, delayBetweenDialogues));
        }

        public void ShowDialogue(string dialogue) // 한 문장 출력
        {
            string[] singleDialogue = new string[] { dialogue };
            ShowDialogue(singleDialogue);
        }

        private IEnumerator DisplayDialogues(string[] dialogues, float delayBetweenDialogues)
        {
            displaying = true;
            float previousScale = Time.timeScale;
            dialogueImage.SetActive(true);
            dialogueCharacter.SetActive(true);
            Time.timeScale = 0f;

            foreach (string dialogue in dialogues)
            {
                yield return StartCoroutine(TypeDialogue(dialogue));
                yield return new WaitForSecondsRealtime(delayBetweenDialogues);
            }

            dialogueImage.SetActive(false);
            dialogueCharacter.SetActive(false);
            if (!Lucy.Presentation.GameSession.BlocksGameplayInput) Time.timeScale = previousScale;
            displaying = false;
        }

        private IEnumerator TypeDialogue(string dialogue)
        {
            dialogueText.text = "";

            foreach (char letter in dialogue.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
        }
    }
}
