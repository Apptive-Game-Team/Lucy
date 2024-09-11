using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RemoteController : MonoBehaviour
{
    public ItemData item;
    public GameObject Text;

    public List<Image> seperatingImages;

    private void Start()
    {
        Text.SetActive(false);
        OffAllImages();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(true);
            //여기서 다이어로그 출력(리모콘 인것 같다)
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if(InputManager.Instance.GetKeyDown(ActionCode.Interaction))
            {
                Inventory.instance.AddItem(item);
                //StartCoroutine(GetBatteryEvent());
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

    private void OffAllImages()
    {
        for(int i = 0; i < seperatingImages.Count; i++)
        {
            seperatingImages[i].gameObject.SetActive(false);
        }
    }

    public void GetBatteryEventCoroutine()
    {
        StartCoroutine(GetBatteryEvent());
    }

    private IEnumerator GetBatteryEvent()
    {
        for(int i = 0; i < seperatingImages.Count; i++)
        {
            seperatingImages[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            seperatingImages[i].gameObject.SetActive(false);
        }
        Inventory.instance.AddItem(item);
        Destroy(gameObject);
    }
}
