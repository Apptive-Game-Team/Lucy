using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureController : SingletonObject<FurnitureController>
{
    public Dictionary<FurnitureType, Furnitures> furnitures;
    public GameObject player;
    public GameObject flashlight;
    public List<Sprite> drawerImages;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        player = Character.Instance.gameObject;
        flashlight.SetActive(false);
        furnitures = new Dictionary<FurnitureType, Furnitures>()
        {
            { FurnitureType.Cabinet, new Cabinet(player)},
            { FurnitureType.Bookshelf, new Bookshelf()},
            { FurnitureType.Drawer, new Drawer(flashlight,drawerImages)}
        };
    }

    /*void Update()
    {
        if (!player.activeSelf && InputManager.Instance.GetKeyDown(ActionCode.Interaction))
        {
            player.SetActive(true);
        }
    }*/
}

public abstract class Furnitures
{
    public abstract void Interact(Furniture furniture);
}

public class Cabinet : Furnitures
{
    private GameObject player;
    private bool isHidden = false;

    public Cabinet(GameObject player)
    {
        this.player = player;
    }

    private IEnumerator Reveal()
    {
        InputManager.Instance.GetKeyDown(ActionCode.Interaction);
        yield return new WaitUntil(() =>
        {
            if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
            {
                player.SetActive(true);
                return true;
            }
            return false;
        });
        yield return new WaitForSeconds(0.2f);
        isHidden = false;
    }

    public override void Interact(Furniture furniture)
    {
        if (!isHidden)
        {
            player.SetActive(false);
            isHidden = true;
            FurnitureController.Instance.StartCoroutine(Reveal());
        }
    }
}

public class Bookshelf : Furnitures
{
    public override void Interact(Furniture furniture)
    {
        GameObject bookPage = furniture.transform.Find("Canvas").gameObject;

        if (!bookPage.activeSelf)
        {
            bookPage.SetActive(true);
            InputManager.Instance.SetMovementState(false);
        }
        else
        {
            bookPage.SetActive(false);
            InputManager.Instance.SetMovementState(true);
        }
    }
}

public class Drawer : Furnitures
{
    private GameObject flashlight;
    private List<Sprite> drawerImages;
    private SpriteRenderer spriteRenderer;

    public Drawer(GameObject flashlight,List<Sprite> drawerImages)
    {
        this.flashlight = flashlight;
        this.drawerImages = drawerImages;
        spriteRenderer = GameObject.Find("Drawer").GetComponent<SpriteRenderer>();
    }

    public override void Interact(Furniture furniture)
    {
        if (flashlight == null || !flashlight.activeSelf)
        {
            if (spriteRenderer.sprite == drawerImages[0])
            {
                spriteRenderer.sprite = drawerImages[1];
                if (flashlight != null)
                {
                    flashlight.SetActive(true);
                }
            }
            else if (spriteRenderer.sprite == drawerImages[1])
            {
                spriteRenderer.sprite = drawerImages[0];
            }
        }
        else
        {
            Inventory.instance.AddItem(flashlight.GetComponent<ItemObject>().item);
            flashlight.SetActive(false);
            flashlight = null;
        }
    }
}

