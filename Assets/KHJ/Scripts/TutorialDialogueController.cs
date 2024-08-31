using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Tutorial
{
    public class TutorialDialogueController : MonoBehaviour
    {
        public TMP_Text dialogueText;

        public void DialogueStream(string text)
        {
            StartCoroutine(UpdateDialogueStream(text + " "));
        }

        IEnumerator UpdateDialogueStream(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                yield return new WaitForSeconds(0.06f);
                dialogueText.SetText(text.Substring(0, i));
            }
            
            yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)));
        }
    }
}
