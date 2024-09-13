using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTile : MonoBehaviour
{
    [SerializeField] private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D player)
    {
        PuzzleManager.instance.isSafe = true;
        FlashLight.instance.consumeSpeed = 1;
        CharacterStat.instance.reduceSpeed = 1;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        PuzzleManager.instance.isSafe = false;
        FlashLight.instance.consumeSpeed = 3;
        CharacterStat.instance.reduceSpeed = 3;
    }
}
