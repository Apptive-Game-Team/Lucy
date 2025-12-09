using invertoryAndItem;
using UnityEngine;

namespace Puzzle
{
    public class FloorTile : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                FlashLight.instance.consumeAmount = 1;
                CharacterStat.instance.reduceAmount = 10;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                FlashLight.instance.consumeAmount = 3;
                CharacterStat.instance.reduceAmount = 30;
            }
        }
    }
}
