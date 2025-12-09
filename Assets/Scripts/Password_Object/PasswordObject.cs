using UnityEngine;

namespace Password_Object
{
    public class PasswordObject : MonoBehaviour
    {

        [SerializeField] protected string password = "";

        [SerializeField] PasswordSystem passwordSystem;

        protected void OpenPasswordPage()
        {
            passwordSystem.SetPassword(password, this);
        } 

        public virtual void Unlock()
        {
            Debug.Log("unlocked by password");
        }
    }
}
