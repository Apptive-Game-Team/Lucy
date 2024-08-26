using System;
using System.Collections;
using UnityEngine;

namespace Dialogue
{
    public class PlayerController : MonoBehaviour
    {
        public int GetOutFirstRoom = 0;
        public int GetInRightDownRoom = 0;
        public int GetInRightUpRoom = 0;
        public int CheckStairRoom = 0;

        public void SetDialogueFlag(int num)
        {
            switch (num)
            {
                case 0:
                    GetOutFirstRoom++;
                    break;
                case 1:
                    GetInRightDownRoom++;
                    break;
                case 2:
                    GetInRightUpRoom++;
                    break;
                case 3:
                    CheckStairRoom++;
                    break;
            }
        }

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

            if (GetOutFirstRoom == 1 || Input.GetKeyDown(KeyCode.Alpha1)) // 문열고 복도로 나갈때 
            {
                DialogueSystem.Instance.ShowDialogue(DialogueDatas.Instance.GetDialogue("Chapter1","Corrider")[0]);
                GetOutFirstRoom++;
            }

            if (GetInRightDownRoom == 1 || Input.GetKeyDown(KeyCode.Alpha2)) // 오른쪽 방 들어갔을때
            {
                DialogueSystem.Instance.ShowDialogue(DialogueDatas.Instance.GetDialogue("Chapter1","RightRoom")[0]);
                GetInRightDownRoom++;
            }

            if (GetInRightUpRoom == 1 || Input.GetKeyDown(KeyCode.Alpha3)) // 오른쪽 방 위에 들어갈때
            {
                DialogueSystem.Instance.ShowDialogue(DialogueDatas.Instance.GetDialogue("Chapter1","RightRoom")[1]);
                GetInRightUpRoom++;
            } 

            if (CheckStairRoom == 1 || Input.GetKeyDown(KeyCode.Alpha4)) // 나와서 복도에 계단방을 확인할때
            {
                DialogueSystem.Instance.ShowDialogue(DialogueDatas.Instance.GetDialogue("Chapter1","Corrider")[1]);
                CheckStairRoom++;
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
