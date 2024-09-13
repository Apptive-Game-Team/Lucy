using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour
{
    public static FurnitureController Instance { get; private set;}
    public Dictionary<FurnitureType, Furnitures> furnitures;
    public GameObject player;

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
            { FurnitureType.Cabinet, new Cabinet() },
            { FurnitureType.Bookshelf, new Bookshelf() },
            { FurnitureType.Drawer, new Drawer() }
        };
    }

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
    public override void Interact()
    {
        Debug.Log("Bookshelf");
    }
}

public class Drawer : Furnitures
{
    public override void Interact()
    {
        Debug.Log("Drawer");
    }
}

