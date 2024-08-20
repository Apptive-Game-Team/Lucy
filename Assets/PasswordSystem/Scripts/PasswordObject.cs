using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
