using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlicePuzzle
{
    public class PuzzleManager : MonoBehaviour
    {
        public Button[] puzzlePieces;
        public int emptyIndex;
        public Button resetButton;

        private Vector3[] initialPositions;
        private Button[] initialPuzzlePieces;

        void Start()
        {
            emptyIndex = puzzlePieces.Length - 1;

            initialPositions = new Vector3[puzzlePieces.Length];
            initialPuzzlePieces = new Button[puzzlePieces.Length];

            for (int i = 0; i < puzzlePieces.Length; i++)
            {
                initialPositions[i] = puzzlePieces[i].transform.position;
                initialPuzzlePieces[i] = puzzlePieces[i];
            }

            AssignButtonListeners();

            resetButton.onClick.AddListener(ResetPuzzle);
        }

        void AssignButtonListeners()
        {
            for (int i = 0; i < puzzlePieces.Length; i++)
            {
                int index = i;
                puzzlePieces[i].onClick.RemoveAllListeners();
                puzzlePieces[i].onClick.AddListener(() => TryMovePiece(index));
            }
        }

        void TryMovePiece(int pieceIndex)
        {
            if (IsAdjacent(pieceIndex, emptyIndex))
            {
                SwapPieces(pieceIndex, emptyIndex);
                emptyIndex = pieceIndex;
                AssignButtonListeners();

                CheckIfPuzzleSolved();
            }
        }

        bool IsAdjacent(int index1, int index2)
        {
            int x1 = index1 % 3;
            int y1 = index1 / 3;
            int x2 = index2 % 3;
            int y2 = index2 / 3;

            return (Mathf.Abs(x1 - x2) == 1 && y1 == y2)
                || (Mathf.Abs(y1 - y2) == 1 && x1 == x2);
        }

        void SwapPieces(int index1, int index2)
        {
            var button1 = puzzlePieces[index1];
            var button2 = puzzlePieces[index2];

            var tempButtonPosition = button1.transform.position;
            button1.transform.position = button2.transform.position;
            button2.transform.position = tempButtonPosition;

            puzzlePieces[index1] = button2;
            puzzlePieces[index2] = button1;
        }
        void CheckIfPuzzleSolved()
        {
            for (int i = 0; i < puzzlePieces.Length; i++)
            {
                int correctNummber = i + 1;

                if (puzzlePieces[i].name != correctNummber.ToString())
                {
                    return;
                }

            }
            OnPuzzleSolved();
        }

        void OnPuzzleSolved()
        {
            Debug.Log("Puzzle Solved!");
        }

        void ResetPuzzle()
        {
            for (int j=0; j<2; j++)
            {
                for (int i = 0; i < puzzlePieces.Length; i++)
                {
                    puzzlePieces[i].transform.position = initialPositions[i];
                    puzzlePieces[i] = initialPuzzlePieces[i];
                }

                emptyIndex = puzzlePieces.Length - 1;

                AssignButtonListeners();
            }
        }
    }
}