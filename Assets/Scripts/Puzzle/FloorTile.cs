using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTile : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            BridgePuzzleManager.instance.isSafe = true;
            FlashLight.instance.consumeAmount = 1;
            CharacterStat.instance.reduceAmount = 10;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BridgePuzzleManager.instance.isSafe = false;
            FlashLight.instance.consumeAmount = 3;
            CharacterStat.instance.reduceAmount = 30;
        }
    }
}
