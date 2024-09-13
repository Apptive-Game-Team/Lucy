using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PuzzleSwitch2F : MonoBehaviour
{
    [SerializeField] private GameObject floorTile;
    [SerializeField] private GameObject voidTile;
    [SerializeField] private GameObject[] bridges;
    [SerializeField] private GameObject player;
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
        if (other.gameObject == player)
        {
            for (int i = 0; i < bridges.Length; i++)
            {
                bridges[i].SetActive(true);
            }
        }
    }
}
