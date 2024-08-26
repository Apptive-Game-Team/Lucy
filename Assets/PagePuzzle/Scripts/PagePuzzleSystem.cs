using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PagePuzzleSystem : MonoBehaviour
{

    private string puzzle = "";

    PagePuzzle pagePuzzle;

    Canvas pagePuzzleCanvas;

    TMP_Text text;

    private void Awake()
    {
        text = gameObject.GetComponentInChildren<TMP_Text>();
        pagePuzzleCanvas = gameObject.GetComponentInChildren<Canvas>();
    }

    void Start()
    {
        pagePuzzleCanvas.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        puzzle = "";
        text.SetText(puzzle);
    }

    public void ClosePagePuzzle()
    {
        pagePuzzleCanvas.gameObject.SetActive(false);
    }

    public void SetPuzzle(string puzzle, PagePuzzle pagePuzzle)
    { 
        this.pagePuzzle = pagePuzzle;
        pagePuzzleCanvas.gameObject.SetActive(true);
        this.puzzle = puzzle;
        text.SetText(puzzle);
    }
}
