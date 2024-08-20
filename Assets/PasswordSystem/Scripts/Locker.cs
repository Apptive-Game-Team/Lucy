using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locker : PasswordObject
{

    [SerializeField] GameObject printText;

    [SerializeField] GameObject item;

    public override void Unlock()
    {
        base.Unlock();
        Instantiate(item);
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")){
            if (Input.GetKey(KeyCode.Z))
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
