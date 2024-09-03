using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Creature;

public class CreatureManager : MonoBehaviour
{
    private int[,] map;

    private List<Creature.Creature> creatures = new List<Creature.Creature>();
    private MapBuilder mapBuilder;
    private List<Tilemap> tilemaps = new List<Tilemap>();
    private Vector3Int mapOffset;

    private static CreatureManager _instance;
    public static CreatureManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            mapBuilder = gameObject.GetComponent<MapBuilder>();
            tilemaps.Add(GameObject.Find("Floor").GetComponent<Tilemap>());
            tilemaps.Add(GameObject.Find("Furniture").GetComponent<Tilemap>());
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddCreature(Creature.Creature creature)
    {
        creatures.Add(creature);
    }


    public int[,] GetMap()
    {
        if (map == null)
        {
            InitMap();
        }
        return map;
    }

    public Vector3Int GetMapOffset()
    {
        if (mapOffset == null)
        {
            InitMap();
        }
        return mapOffset;
    }

    private void InitMap()
    {
        BoundsInt bounds = tilemaps[0].cellBounds;
        int maxX = 0;
        int maxY = 0;
        int minX = 0;
        int minY = 0;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            maxX = Mathf.Max(position.x, maxX);
            maxY = Mathf.Max(position.y, maxY);
            minX = Mathf.Min(position.x, minX);
            minY = Mathf.Min(position.y, minY);
        }

        maxX += 10;
        maxY += 10;
        minX -= 10;
        minY -= 10;

        map = new int[maxX - minX + 1, maxY - minY + 1];
        mapOffset = new Vector3Int(minX, minY);

        bool isReversed = false;
        foreach (Tilemap tilemap in tilemaps)
        {
             
            foreach (Vector3Int position in bounds.allPositionsWithin)
            {
                TileBase tileBase = tilemap.GetTile(position);
                int offsetAppliedX = position.x - mapOffset.x;
                int offsetAppliedY = position.y - mapOffset.y;

                if (offsetAppliedX > 0 && offsetAppliedX < map.GetLength(0) && offsetAppliedY > 0 && offsetAppliedY < map.GetLength(1))
                {
                    if (!isReversed && tileBase != null)
                    {
                        map[offsetAppliedX, offsetAppliedY] = 1;
                    }
                    else if (isReversed && tileBase != null)
                    {
                        print("tlqkf");
                        map[offsetAppliedX, offsetAppliedY] = 0;
                    }
                }
            }
            isReversed = true;
        }
    }
}
