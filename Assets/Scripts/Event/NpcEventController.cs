using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Creature;
using CharacterCamera;

public class NpcEventController : MonoBehaviour
{
    private GameObject Npc;
    private GameObject guardObj;
    private Guard guard;
    private GameObject player;
    private GameObject barricade;
    private GameObject blackOutImage;
    private SpriteRenderer blackOutImageSpriteRenderer;
    private CharacterMove playerMoveScript;
    private float npcEventTime = 5f;
    private float blackOutDelay = 3f;
    private void Start()
    {
        guardObj = GameObject.Find("Guard");
        guard = guardObj.GetComponent<Guard>(); 

        player = Character.Instance.gameObject;
        playerMoveScript = player.GetComponent<CharacterMove>();

        barricade = GameObject.Find("Barricade");

        blackOutImage = GameObject.Find("blackOut");
        blackOutImageSpriteRenderer = blackOutImage.GetComponent<SpriteRenderer>();

        Npc = GameObject.Find("NPC");

        SetBlackOutImageAlpha(0);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")&& player.transform.position.x < -2)
        {
            StartNpcEvent();
        }
    }

    private void StartNpcEvent()
    {
        playerMoveScript.StopMovement();
        guard.StopPatrol();
        //startdialog
        barricade.SetActive(false);
        StartCoroutine(WaitAndFinishNpcEvent());
    }

    private IEnumerator BlackOutEffect()
    {
        for (float alpha = 0; alpha <= 1; alpha += Time.deltaTime / blackOutDelay)
        {
            SetBlackOutImageAlpha(alpha);
            yield return null;
        }
        yield return new WaitForSeconds(blackOutDelay);
        Destroy(Npc);
        for (float alpha = 1; alpha >= 0; alpha -= Time.deltaTime / blackOutDelay)
        {
            SetBlackOutImageAlpha(alpha);
            yield return null;
        }
        FinishNpcEvent();
    }

    private IEnumerator WaitAndFinishNpcEvent()
    {
        yield return new WaitForSeconds(npcEventTime);
        StartCoroutine(BlackOutEffect());
    }

    private void FinishNpcEvent()
    {
        playerMoveScript.ResumeMovement();
        guard.StartPatrol();
        Destroy(this);
    }

    private void SetBlackOutImageAlpha(float alpha)
    {
        Color color = blackOutImageSpriteRenderer.color;
        color.a = alpha;
        blackOutImageSpriteRenderer.color = color;
    }
}