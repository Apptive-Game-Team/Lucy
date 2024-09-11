using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleSwitch2F : MonoBehaviour
{
    [SerializeField] private GameObject floorTile;
    [SerializeField] private GameObject voidTile;
    [SerializeField] private GameObject bridgeTile;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
       bridgeTile.SetActive(false);
    }
}
