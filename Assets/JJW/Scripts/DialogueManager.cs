using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace dialogue_2
{
    public class DialogueManager : MonoBehaviour
    {
        public TMP_Text dialogueText;
        public Image dialogueBox;
        public Image dialogueCharacter;

        private Queue<string> sentences;

        void Start()
        {
            sentences = new Queue<string>();
            dialogueBox.gameObject.SetActive(false);
        }

        public void StartDialogue(Dialogue dialogue)
        {
            dialogueBox.gameObject.SetActive(true);

            sentences.Clear();

            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }

            DisplayNextSentence();

        }

        public void DisplayNextSentence()
        {
            if (sentences.Count == 0)
            {
                EndDialogue();
                return;
            }

            string sentence = sentences.Dequeue();
            dialogueText.text = sentence;
        }

        void EndDialogue()
        {
            dialogueBox.gameObject.SetActive(false);
        }

    }

    [System.Serializable]
    public class Dialogue
    {
        public string name;
        [TextArea(3, 10)]
        public string[] sentences;
    }

    public class DialogueTrigger : MonoBehaviour
    {
        public Dialogue dialogue;
        private DialogueManager dialogueManager;

        void Start()
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        public void TriggerDialogue()
        {
            dialogueManager.StartDialogue(dialogue);
        }
    }
}

