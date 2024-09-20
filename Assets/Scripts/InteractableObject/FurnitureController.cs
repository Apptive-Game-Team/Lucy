using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FurnitureController : SingletonObject<FurnitureController>, IKeyInputListener, ISceneChangeListener
{
    public Dictionary<FurnitureType, Furnitures> furnitures;
    public GameObject player;
    public GameObject bookPage;
    public GameObject flashlight;
    public List<Sprite> drawerImages;

    public void OnSceneChange()
    {
        bookPage = ReferenceManager.Instance.FindGameObjectByName("EventImages").transform.Find("RemoteController1").gameObject;
    }

    protected override void Awake()
    {
        base.Awake();
        PortalManager.Instance.SetSceneChangeListener(this);
    }

    void Start()
    {
        player = Character.Instance.gameObject;
        flashlight.SetActive(false);
        furnitures = new Dictionary<FurnitureType, Furnitures>()
        {
            { FurnitureType.Cabinet, new Cabinet()},
            { FurnitureType.Bookshelf, new Bookshelf(bookPage)},
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
        try{
                if (bookPage.activeSelf && InputManager.Instance.GetKeyDown(ActionCode.Interaction))
                {
                    bookPage.SetActive(false);
                    Time.timeScale = 1;
                }
        }catch(System.Exception){
            Debug.LogWarning("BookPage is not Attached yet");
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
    private GameObject flashlight;
    private List<Sprite> drawerImages;
    private SpriteRenderer spriteRenderer;

    public Drawer(GameObject flashlight,List<Sprite> drawerImages)
    {
        this.flashlight = flashlight;
        this.drawerImages = drawerImages;
        spriteRenderer = GameObject.Find("Drawer").GetComponent<SpriteRenderer>();
    }

    public override void Interact()
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
        Debug.Log("Drawer");
    }
}

