using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{

    [SerializeField] GameObject wall;
    int[,] grid = new int[,]
    {
        {1, 1, 1, 1, 1, 1,1, 1, 1},
        { 1, 0, 1, 0, 0, 0, 0, 0 ,1},
        {1,  0, 1, 0, 1, 1, 1, 0 ,1},
        {1,  0, 1, 0, 0, 0, 0, 0 ,1},
        {1,  0, 0, 0, 1, 0, 1, 0 ,1},
        {1,  0, 1, 0, 1, 0, 1, 0 ,1 },
        {1,  0, 1, 0, 0, 0, 0, 0 ,1},
        {1, 0, 1, 0, 1, 1, 1, 0 ,1},
        {1, 0, 0, 0, 0, 0, 0, 0 ,1},
        {1, 1, 1, 1, 1, 1,1, 1, 1}
    };

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i<grid.GetLength(0); i++)
        {
            for (int j = 0; j<grid.GetLength(1); j++)
            {
                if (grid[i, j] == 1)
                {
                    MakeWall(i, j);
                }
                    
            }
        }
    }

    void MakeWall(int x, int y)
    {
        Instantiate(wall, new Vector3(x-8, y-4), Quaternion.identity);
    }
}
