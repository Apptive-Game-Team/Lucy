using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locker : PasswordObject
{

    [SerializeField] GameObject printText;

    public override void Unlock()
    {
        base.Unlock();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")){
            if (Input.GetKeyDown(KeyCode.Z))
            {
                OpenPasswordPage();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            printText.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            printText.SetActive(false);
    }   

}
