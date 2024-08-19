using System;
using System.Collections;
using UnityEngine;

namespace Dialogue
{
    public class DialogueController2 : MonoBehaviour
    {
        public Boolean GetOutFirstRoom = false;
        public Boolean GetInRightDownRoom = false;
        public Boolean GetInRightUpRoom = false;
        public Boolean CheckStairRoom = false;

        void Start()
        {
            StartCoroutine(StartDialogue()); // 처음 시작할때 스크립트 재생
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DialogueSystem2.Instance.ShowDialogue(DialogueDatas2.Instance.GetDialogue("Chapter1","FirstRoom")[1]);
            }

            if (GetOutFirstRoom || Input.GetKeyDown(KeyCode.Alpha1)) // 문열고 복도로 나갈때 
            {
                DialogueSystem2.Instance.ShowDialogue(DialogueDatas2.Instance.GetDialogue("Chapter1","Corrider")[0]);
            }

            if (GetInRightDownRoom || Input.GetKeyDown(KeyCode.Alpha2)) // 오른쪽 방 들어갔을때
            {
                DialogueSystem2.Instance.ShowDialogue(DialogueDatas2.Instance.GetDialogue("Chapter1","RightRoom")[0]);
            }

            if (GetInRightUpRoom || Input.GetKeyDown(KeyCode.Alpha3)) // 오른쪽 방 위에 들어갈때
            {
                DialogueSystem2.Instance.ShowDialogue(DialogueDatas2.Instance.GetDialogue("Chapter1","RightRoom")[1]);
            } 

            if (CheckStairRoom || Input.GetKeyDown(KeyCode.Alpha4)) // 나와서 복도에 계단방을 확인할때
            {
                DialogueSystem2.Instance.ShowDialogue(DialogueDatas2.Instance.GetDialogue("Chapter1","Corrider")[1]);
            }
        }

        private IEnumerator StartDialogue()
        {
            yield return WaitDelay(4f);
            DialogueSystem2.Instance.ShowDialogue(DialogueDatas2.Instance.GetDialogue("Chapter1","FirstRoom")[0]);
        }

        private IEnumerator WaitDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}
