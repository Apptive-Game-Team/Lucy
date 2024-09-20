using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FurnitureController : SingletonObject<FurnitureController>, IKeyInputListener
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
            { FurnitureType.Cabinet, new Cabinet()},
            { FurnitureType.Bookshelf, new Bookshelf()},
            { FurnitureType.Drawer, new Drawer(flashlight,drawerImages)}
        };
        InputManager.Instance.SetKeyListener(this);
    }

    //void IKeyInputListener.OnKeyDown(ActionCode action)
    void Update()
    {
        if (!player.activeSelf && InputManager.Instance.GetKeyDown(ActionCode.Interaction))
        {
            player.SetActive(true);
        }
    }
}

public abstract class Furnitures
{
    public abstract void Interact(Furniture furniture);
}

public class Cabinet : Furnitures
{
    public override void Interact(Furniture furniture)
    {
        GameObject.Find("Player").SetActive(false);
    }
}

public class Bookshelf : Furnitures
{
    public override void Interact(Furniture furniture)
    {
        GameObject bookPage = furniture.transform.Find("Canvas").Find("Image").gameObject;

        if (!bookPage.activeSelf)
        {
            bookPage.SetActive(true);
            InputManager.Instance.MoveControl(false);
        }
        else
        {
            bookPage.SetActive(false);
            InputManager.Instance.MoveControl(true);
        }
        
        Debug.Log("Bookshelf");
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
        Debug.Log("Drawer");
    }
}

