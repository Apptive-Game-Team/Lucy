using InputSystem;
using UnityEngine;

namespace Password_Object
{
    public class Locker : PasswordObject
    {

        [SerializeField] GameObject printText;

        [SerializeField] GameObject item;
        private bool unlocked;

        public override void Unlock()
        {
            if (unlocked) return;
            unlocked = true;
            if (printText != null) printText.SetActive(false);
            base.Unlock();
            if (item != null) Instantiate(item, (Vector3)transform.position + Vector3.down, Quaternion.identity);
        
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!unlocked && collision.CompareTag("Player") && InputManager.Instance != null){
                if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
                {
                    OpenPasswordPage();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!unlocked && printText != null && collision.CompareTag("Player"))
                printText.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!unlocked && printText != null && collision.CompareTag("Player"))
                printText.SetActive(false);
        }   

    }
}
