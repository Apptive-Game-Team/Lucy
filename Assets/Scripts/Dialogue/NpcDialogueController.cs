using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum NpcType
{
    PUZZLENPC,
}

public abstract class NpcDialogues
{
    public abstract void Show();
}

public class puzzleNpc : NpcDialogues
{
    private NpcDialogueData npcDialogueData;
    public puzzleNpc(NpcDialogueData npcDialogueData)
    {
        this.npcDialogueData = npcDialogueData;
    }

    public override void Show()
    {
        NpcDialogueController.Instance.ShowDialogue(npcDialogueData.GetDialogues(NpcType.PUZZLENPC.ToString()));
    }
}

public class NpcDialogueController : SingletonObject<NpcDialogueController>, ISceneChangeListener
{
    public NpcDialogueData npcDialogueData;
    protected List<NpcDialogues> npcDialogue;

    private ReferableObject dialogueImages;
    public GameObject dialogueImage;
    public GameObject dialogueCharacter;
    public TMP_Text dialogueText;

    public float typingSpeed = 0.05f;
    public float delayBetweenDialogues = 1.2f;

    protected override void Awake()
    {
        base.Awake();
        PortalManager.Instance.SetSceneChangeListener(this);
    }

    void ISceneChangeListener.OnSceneChange()
    {
        dialogueImages =  ReferenceManager.Instance.FindComponentByName<ReferableObject>("DialogueImages");

        dialogueImage = dialogueImages.transform.Find("DialogueBackground").gameObject;
        dialogueCharacter = dialogueImages.transform.Find("LucyDialogueImage").gameObject;
        dialogueText = dialogueImages.GetComponentInChildren<TMP_Text>();
        dialogueImage.SetActive(false);
        dialogueCharacter.SetActive(false);

        npcDialogue = new List<NpcDialogues>()
        {
            new puzzleNpc(npcDialogueData)
        };
    }

    public void ShowDialogue(string[] dialogues, float delayBetweenDialogues = 1.5f) // 두 문장 이상 출력
    {
        StartCoroutine(DisplayDialogues(dialogues, delayBetweenDialogues));
    }

    public void ShowDialogue(string dialogue) // 한 문장 출력
    {
        string[] singleDialogue = new string[] { dialogue };
        ShowDialogue(singleDialogue);
    }

    private IEnumerator DisplayDialogues(string[] dialogues, float delayBetweenDialogues)
    {
        dialogueImage.SetActive(true);
        dialogueCharacter.SetActive(true);
        Time.timeScale = 0f;

        foreach (string dialogue in dialogues)
        {
            yield return StartCoroutine(TypeDialogue(dialogue));
            yield return new WaitForSecondsRealtime(delayBetweenDialogues);
        }

        dialogueImage.SetActive(false);
        dialogueCharacter.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator TypeDialogue(string dialogue)
    {
        dialogueText.text = "";

        foreach (char letter in dialogue.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }
}
