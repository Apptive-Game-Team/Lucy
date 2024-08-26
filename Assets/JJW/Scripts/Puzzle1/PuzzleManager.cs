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

        void Start()
        {
            emptyIndex = puzzlePieces.Length - 1;

            AssignButtonListeners();
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
            }
        }

        bool IsAdjacent(int index1, int index2)
        {
            int x1 = index1 % 3;
            int y1 = index1 / 3;
            int x2 = index2 % 3;
            int y2 = index2 / 3;

            return (Mathf.Abs(x1 - x2) == 1 && y1 == y2) || (Mathf.Abs(y1 - y2) == 1 && x1 == x2);
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
    }
}
