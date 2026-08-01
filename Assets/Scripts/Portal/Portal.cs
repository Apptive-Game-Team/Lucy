using UnityEngine;

namespace Portal
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] PortalID portalID;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player"))
            {
                return;
            }

            // Run on PortalManager: this Portal is destroyed by the scene load, which would
            // abort the coroutine before the player is repositioned and the screen faded back in.
            PortalManager.Instance.StartCoroutine(PortalManager.Instance.TransitScene(portalID));
        }
    }
}
