using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcDialogue", menuName = "New NpcDialogue")]
public class NpcDialogueData : ScriptableObject
{
    [Serializable]
    public class NpcType
    {
        public string npcType;
        public List<Text> texts;
    }

    [Serializable]
    public class Text
    {
        public string text;
    }

    public List<NpcType> npcDialogues;

    public string[] GetDialogues(string npcType)
    {
        NpcType npcData = npcDialogues.Find(c => c.npcType == npcType);
        if (npcData != null)
        {
            string[] result = new string[npcData.texts.Count];
            for (int i = 0; i < npcData.texts.Count; i++)
            {
                result[i] = npcData.texts[i].text;
            }
            return result;
        }

        Debug.LogWarning($"Npc {npcType} 대사가 존재하지 않음");
        return null;
    }

}
