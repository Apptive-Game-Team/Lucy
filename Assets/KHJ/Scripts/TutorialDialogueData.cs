using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tutorial
{
[CreateAssetMenu]
    public class TutorialDialogueData : ScriptableObject
    {
        [Serializable] 
        public class Dialogues
        {
            public string text;
            public int background;
        }

        public List<Dialogues> dialogues = new();

        public string GetDialogue(int num)
        {
            return dialogues[num].text;
        }

        public int GetDialogueLength()
        {
            return dialogues.Count;
        }
    }
}
