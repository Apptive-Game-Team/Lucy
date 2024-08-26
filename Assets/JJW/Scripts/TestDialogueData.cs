using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/Dialogue Data", order = 1)]
    public class TestDialogueData : ScriptableObject
    {
        public static TestDialogueData Instance  {get; private set;}

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        [System.Serializable]
        public class SceneDialogue
        {
            public string sceneName;
            public string[][] dialogues;
        }

        [System.Serializable]
        public class ChapterDialogue
        {
            public string chapterName;
            public List<SceneDialogue> scenes;
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
                    return sceneData.dialogues;
                }
            }

            Debug.LogWarning($"대사 데이터를 찾을 수 없습니다: {chapter} - {scene}");
            return null;
        }
    }
}
