using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Creature;

public class CreatureManager : MonoBehaviour
{
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
            player = GameObject.FindWithTag("Player").GetComponent<Player>();
            tilemap = GameObject.Find("Floor").GetComponent<Tilemap>();
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private int[,] map;

    private Player player;
    private List<Creature.Creature> creatures = new List<Creature.Creature>();
    private MapBuilder mapBuilder;
    private Tilemap tilemap;
    private Vector3Int mapOffset;

    public void AddCreature(Creature.Creature creature)
    {
        creatures.Add(creature);
    }

    public Player GetPlayer()
    {
        return player;
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
        BoundsInt bounds = tilemap.cellBounds;
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

        map = new int[maxX - minX + 1, maxY - minY + 1];
        mapOffset = new Vector3Int(minX, minY);

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase tileBase = tilemap.GetTile(position);
            if (tileBase == null)
            {
                map[position.x - mapOffset.x, position.y - mapOffset.y] = 1;
            }
            else
            {
                map[position.x - mapOffset.x, position.y - mapOffset.y] = 0;
            }
        }
    }
}
