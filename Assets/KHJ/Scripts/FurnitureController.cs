using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FurnitureController : MonoBehaviour, IKeyInputListener
{
    public static FurnitureController Instance { get; private set;}
    public Dictionary<FurnitureType, Furnitures> furnitures;
    public GameObject player;
    public GameObject bookPage;
    public GameObject key;
    public List<Sprite> drawerImages;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        furnitures = new Dictionary<FurnitureType, Furnitures>()
        {
            { FurnitureType.Cabinet, new Cabinet()},
            { FurnitureType.Bookshelf, new Bookshelf(bookPage)},
            { FurnitureType.Drawer, new Drawer(key,drawerImages)}
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

        if (bookPage.activeSelf && InputManager.Instance.GetKeyDown(ActionCode.Interaction))
        {
            bookPage.SetActive(false);
            Time.timeScale = 1;
        }
    }
}

public abstract class Furnitures
{
    public abstract void Interact();
}

public class Cabinet : Furnitures
{
    public override void Interact()
    {
        GameObject.Find("Player").SetActive(false);
    }
}

public class Bookshelf : Furnitures
{
    private GameObject bookPage;

    public Bookshelf(GameObject bookPage)
    {
        this.bookPage = bookPage;
    }
    
    public override void Interact()
    {
        bookPage.SetActive(true);
        Time.timeScale = 0;
        Debug.Log("Bookshelf");
    }
}

public class Drawer : Furnitures
{
    private GameObject key;
    private List<Sprite> drawerImages;
    private SpriteRenderer spriteRenderer;

    public Drawer(GameObject key,List<Sprite> drawerImages)
    {
        this.key = key;
        this.drawerImages = drawerImages;
        spriteRenderer = GameObject.Find("Drawer").GetComponent<SpriteRenderer>();
    }

    public override void Interact()
    {
        if (key == null || !key.activeSelf)
        {
            if (spriteRenderer.sprite == drawerImages[0])
            {
                spriteRenderer.sprite = drawerImages[1];
                if (key != null)
                {
                    key.SetActive(true);
                }
            }
            else if (spriteRenderer.sprite == drawerImages[1])
            {
                spriteRenderer.sprite = drawerImages[0];
            }
        }
        Debug.Log("Drawer");
    }
}

