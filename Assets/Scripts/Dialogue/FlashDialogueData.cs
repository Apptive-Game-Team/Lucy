using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FlashDialogue
{
    [CreateAssetMenu(fileName = "FlashDialogueData")]
    public class FlashDialogueData : ScriptableObject
    {
        [Serializable]
        public class FlashDialogue
        {
            public string[] texts;
        }

        public List<FlashDialogue> dialogues = new();
    }

}
