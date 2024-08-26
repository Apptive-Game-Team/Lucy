using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/Dialogue Data", order = 1)]
    public class TestDialogueData : ScriptableObject
    {
        [System.Serializable]
        public class ChapterDialogue
        {
            public string chapterName;
            public List<SceneDialogue> scenes;
        }

        [System.Serializable]
        public class SceneDialogue
        {
            public string sceneName;
            public List<Dialogue> dialogues;
        }

        [System.Serializable]
        public class Dialogue
        {
            public string[] dialogueLines;
        }

        public List<ChapterDialogue> chapterDialogues;

        public string[][] GetDialogue(string chapter, string scene)
        {
            ChapterDialogue chapterData = chapterDialogues.Find(c => c.chapterName == chapter);
            if (chapterData != null)
            {
                SceneDialogue sceneData = chapterData.scenes.Find(s => s.sceneName == scene);
                if (sceneData != null)
                {
                    string[][] result = new string[sceneData.dialogues.Count][];
                    for (int i = 0; i < sceneData.dialogues.Count; i++)
                    {
                        result[i] = sceneData.dialogues[i].dialogueLines;
                    }
                    return result;
                }
            }

            Debug.LogWarning($"대사 데이터를 찾을 수 없습니다: {chapter} - {scene}");
            return null;
        }
    }
}
