using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{

    [SerializeField] GameObject wall;
    private CreatureManager creatureManager;

    
    void Start()
    {
        creatureManager = ReferenceManager.Instance.FindByName("CreatureManager") as CreatureManager;
        int[,] map = creatureManager.GetMap();
        Vector3Int mapOffset = creatureManager.GetMapOffset();
        for (int i = 0; i< map.GetLength(0); i++)
        {
            for (int j = 0; j< map.GetLength(1); j++)
            {
                if (map[i, j] == 0)
                {
                    MakeWall(i+mapOffset.x, j+mapOffset.y);
                }
                    
            }
        }
    }

    void MakeWall(int x, int y)
    {
        Instantiate(wall, new Vector3(x, y), Quaternion.identity);
    }
}
