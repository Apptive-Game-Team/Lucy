using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteController : MonoBehaviour
{
    public ItemData item;
    public GameObject Text;

    private void Start()
    {
        Text.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(true);
            //여기서 스크립트 출력(리모콘 인것 같다)
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if(InputManager.Instance.GetKeyDown(ActionCode.Interaction))
            {
                //여기서 스크립트(배터리 빼는 이미지)
                Inventory.instance.AddItem(item);
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(false);
        }
    }
}
