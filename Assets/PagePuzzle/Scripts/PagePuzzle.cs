using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PagePuzzle : MonoBehaviour
{
    [SerializeField] private GameObject printText;
    [SerializeField] private string puzzle = "이거는 ?";

    [SerializeField] PagePuzzleSystem pagePuzzleSystem;

    protected void OpenPuzzlePage()
    {
        pagePuzzleSystem.SetPuzzle(puzzle, this);
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
            {
                OpenPuzzlePage();
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            printText.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            printText.SetActive(false);
    }
}
