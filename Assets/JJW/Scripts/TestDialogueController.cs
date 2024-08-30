using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public enum Floor
    {
        F3
    }

    public enum Room
    {
        FirstRoom,
        Corrider,
        RightRoom,
        StairRoom
    }

    public abstract class DialoguePick
    {
        protected TestDialogueData testDialogueData;
        public DialoguePick(TestDialogueData data)
        {
            testDialogueData = data;
        }
        public abstract void Show();
    }

    public class FirstRoomDialogue : DialoguePick
    {
        bool isFirst = true;
        public FirstRoomDialogue(TestDialogueData data) : base(data) {}
        
        public override void Show()
        {
            if (isFirst)
            {
                TestDialogueSystem.Instance.ShowDialogue(testDialogueData.GetDialogue(Floor.F3.ToString(),Room.FirstRoom.ToString())[1]);
                isFirst = false;
            }
            return;
        }
    }

    public class CorriderDialogue : DialoguePick
    {
        bool isFirst = true;
        public CorriderDialogue(TestDialogueData data) : base(data) {}
        
        public override void Show()
        {
            if (isFirst)
            {
                TestDialogueSystem.Instance.ShowDialogue(testDialogueData.GetDialogue(Floor.F3.ToString(),Room.Corrider.ToString())[0]);
                isFirst = false;
            }
            return;
        }
    }

    public class RightRoomDialogue : DialoguePick
    {
        bool isFirst = true;
        public RightRoomDialogue(TestDialogueData data) : base(data) {}
        
        public override void Show()
        {
            if (isFirst)
            {
                TestDialogueSystem.Instance.ShowDialogue(testDialogueData.GetDialogue(Floor.F3.ToString(),Room.RightRoom.ToString())[0]);
                isFirst = false;
            }
            return;
        }
    }

    public class StairRoomDialogue : DialoguePick
    {
        bool isFirst = true;
        public StairRoomDialogue(TestDialogueData data) : base(data) {}
        
        public override void Show()
        {
            if (isFirst)
            {
                TestDialogueSystem.Instance.ShowDialogue(testDialogueData.GetDialogue(Floor.F3.ToString(),Room.Corrider.ToString())[1]);
                isFirst = false;
            }
            return;
        }
    }
    

    public class TestDialogueController : MonoBehaviour
    {
        

        public TestDialogueData testDialogueData;
        public Floor currentFloor = 0;
        protected List<DialoguePick> dialoguePick;

        public void SetDialogueFlag(int num)
        {
            dialoguePick[num].Show();
        }

        void Start()
        {
            //StartCoroutine(StartDialogue()); // 처음 시작할때 스크립트 재생
            dialoguePick = new List<DialoguePick>()
            {
                new FirstRoomDialogue(testDialogueData),
                new CorriderDialogue(testDialogueData),
                new RightRoomDialogue(testDialogueData),
                new StairRoomDialogue(testDialogueData)
            };
        }

        private IEnumerator StartDialogue()
        {
            yield return WaitDelay(4f);
            TestDialogueSystem.Instance.ShowDialogue(testDialogueData.GetDialogue("F3","FirstRoom")[0]);
        }

        private IEnumerator WaitDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}

