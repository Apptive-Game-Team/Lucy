using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour
{
    public static FurnitureController Instance { get; private set;}
    public Dictionary<FurnitureType, Furnitures> furnitures;

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
}

public abstract class Furnitures
{
    public abstract void Interact();
}

public class Cabinet : Furnitures
{
    public override void Interact()
    {
        Debug.Log("Cabinet");
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

