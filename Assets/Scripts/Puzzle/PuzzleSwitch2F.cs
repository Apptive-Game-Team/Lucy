using SlicePuzzle;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleSwitch2F : MonoBehaviour
{
    [SerializeField] BridgeID bridgeID;
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            PuzzleManager2F.Instance.ActivateBridge(bridgeID);
        }
    }
}
