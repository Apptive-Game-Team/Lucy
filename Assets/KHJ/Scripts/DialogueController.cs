using System.Collections;
using UnityEngine;

namespace Dialogue
{
    public class PlayerController : MonoBehaviour
    {
        void Start()
        {
            StartCoroutine(StartDialogue()); // 처음 시작할때 스크립트 재생
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DialogueSystem.Instance.ShowDialogue(DialogueDatas.Instance.GetDialogue("Chapter1","FirstRoom")[1]);
            }
        }

        private IEnumerator StartDialogue()
        {
            yield return WaitDelay(4f);
            DialogueSystem.Instance.ShowDialogue(DialogueDatas.Instance.GetDialogue("Chapter1","FirstRoom")[0]);
        }

        private IEnumerator WaitDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}
