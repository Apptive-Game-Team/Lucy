using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class DoorInfo
{
    public int keyId;
    public bool isLocked;
    public bool isOpened;
}

public class DoorObject : MonoBehaviour
{
    public GameObject Text;
    [SerializeField] DoorInfo doorInfo;
    [SerializeField] GameObject door;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(true);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if(!doorInfo.isLocked)
                {   
                    door.SetActive(!door.activeSelf);
                }
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
