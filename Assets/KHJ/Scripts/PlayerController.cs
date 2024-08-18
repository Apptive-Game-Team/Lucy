using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public DialogueSystem dialogueSystem;

    void Start()
    {
        StartCoroutine(StartDialogue());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dialogueSystem.ShowDialogue("이건 뭐지?");
        }
    }

    private IEnumerator StartDialogue()
    {
        yield return WaitDelay(4f);
        dialogueSystem.ShowDialogue(new string[] {"?","여긴.. 어디지..?"});
    }

    private IEnumerator WaitDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
    }
}
