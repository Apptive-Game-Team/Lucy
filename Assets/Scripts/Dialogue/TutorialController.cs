using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        public TutorialDialogueController tutorialDialogueController;
        public TutorialDialogueData tutorialDialogueData;
        public List<GameObject> background;

        private void Start()
        {
            background[0].SetActive(true);
            StartCoroutine(TutorialStoryTelling());
        }


        IEnumerator TutorialStoryTelling()
        {
            for (int i = 0; i < tutorialDialogueData.GetDialogueLength(); i++)
            {
                tutorialDialogueController.DialogueStream(tutorialDialogueData.GetDialogue(i));
                yield return new WaitForSeconds(tutorialDialogueData.GetDialogue(i).Length * 0.06f);
                yield return new WaitUntil(() => (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)));
            }

            //LoadGameScene();
        }

        private void LoadGameScene()
        {
            SceneManager.LoadScene("...");
        }
    }
}


