using UnityEngine;

namespace Puzzle
{
    public class PuzzleSwitch2F : MonoBehaviour
    {
        [SerializeField] BridgeID bridgeID;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Tag instead of GameObject.Find("Player"): the name lookup missed the player
            // whenever the collider sat on a child object or the object was named differently.
            if (other.CompareTag("Player"))
            {
                PuzzleManager2F.Instance.ActivateBridge(bridgeID);
            }
        }
    }
}
