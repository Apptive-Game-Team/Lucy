using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{

    [SerializeField] GameObject wall;
    

    
    void Start()
    {
        int[,] map = CreatureManager.Instance.GetMap();
        Vector3Int mapOffset = CreatureManager.Instance.GetMapOffset();
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
