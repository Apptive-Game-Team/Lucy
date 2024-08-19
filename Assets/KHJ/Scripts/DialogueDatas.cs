using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class DialogueDatas : MonoBehaviour
    {
        public static DialogueDatas Instance { get; private set; }

        private Dictionary<string, Dictionary<string, string[][]>> dialogueData;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDialogueData(); // 대사 데이터 초기화
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDialogueData()
        {
            dialogueData = new Dictionary<string, Dictionary<string, string[][]>>();

            dialogueData["Chapter1"] = new Dictionary<string, string[][]>
            {
                {
                    "FirstRoom", new string[][]
                    {
                        new string[] { "?", "여긴.. 어디지..?" },
                        new string[] { "이건 뭐지?" }
                    }
                },
                {
                    "Corrider", new string[][]
                    {
                        new string[] { "복도입니다." },
                        new string[] { "계단방이 있네요."}
                    }
                },
                {
                    "RightRoom", new string[][]
                    {
                        new string[] {"오른쪽방 아래입니다."},
                        new string[] {"오른쪽방 위입니다."}
                    }
                },
            };
        }

        public string[][] GetDialogue(string chapter, string scene)
        {
            if (dialogueData.ContainsKey(chapter) && dialogueData[chapter].ContainsKey(scene))
            {
                return dialogueData[chapter][scene];
            }

            Debug.LogWarning($"대사 데이터를 찾을 수 없습니다: {chapter} - {scene}");
            return null;
        }
    }
}
