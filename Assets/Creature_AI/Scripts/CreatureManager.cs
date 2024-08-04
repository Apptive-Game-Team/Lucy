using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
            mapBuilder = GameObject.Find("Event").GetComponent<MapBuilder>();
            player = GameObject.FindWithTag("Player").GetComponent<Player>();
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private int[,] map = new int[,]
    {
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 0, 1, 0, 0, 0, 0, 0 ,1},
        {1, 0, 1, 0, 1, 1, 1, 0 ,1},
        {1, 0, 1, 0, 0, 0, 0, 0 ,1},
        {1, 0, 0, 0, 1, 0, 1, 0 ,1},
        {1, 0, 1, 0, 1, 0, 1, 0 ,1},
        {1, 0, 1, 0, 0, 0, 0, 0 ,1},
        {1, 0, 1, 0, 1, 1, 1, 0 ,1},
        {1, 0, 0, 0, 0, 0, 0, 0 ,1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1}
    };

    private Player player;
    private List<Creature> creatures = new List<Creature>();
    private MapBuilder mapBuilder;

    public void AddCreature(Creature creature)
    {
        creatures.Add(creature);
    }

    public Player GetPlayer()
    {
        return player;
    }

    public int[,] GetMap()
    {
        return map;
    }
}
