using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.Rendering.Universal;
using static Team6203.Util;

public enum PathFinderType
{
    DEFAULT=0,
    AVOIDER=1,
}

public class CreatureManager : MonoBehaviour
{
    private int[,] map;
    private int[,] doorAppliedMap;
    private int[,] doorAndlightAppliedMap;

    private List<Creature.Creature> creatures = new List<Creature.Creature>();
    private MapBuilder mapBuilder;
    private List<Tilemap> tilemaps = new List<Tilemap>();
    private Vector3Int mapOffset;
    GameObject[] lastSpotLights = new GameObject[0];
    GameObject[] lastDoors = new GameObject[0];

    [SerializeField] bool debugMode;

    public List<PathFinder> pathFinders = new List<PathFinder>();

    private void Awake()
    {
        ReferenceManager.Instance.SetReferableObject("CreatureManager", this, false);
        mapBuilder = gameObject.GetComponent<MapBuilder>();
        tilemaps.Add(GameObject.Find("Floor_tilemap").GetComponent<Tilemap>());
        //tilemaps.Add(GameObject.Find("Furniture_grid").GetComponent<Tilemap>());
        InitMap();
        InitPathFinders();
    }

    void InitPathFinders()
    {
        pathFinders.Add(new PathFinder(GetDoorAppliedMap(), mapOffset, debugMode));
        pathFinders.Add(new PathFinder(GetDoorAndLightAppliedMap(), mapOffset, debugMode));
    }

    public void AddCreature(Creature.Creature creature)
    {
        creatures.Add(creature);
    }


    public int[,] GetMap()
    {
        return map;
    }

    public Vector3Int GetMapOffset()
    {
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
                        map[offsetAppliedX, offsetAppliedY] = 0;
                    }
                }
            }
            isReversed = true;
        }
    }

    public void UpdateMap()
    {
        pathFinders[(int)PathFinderType.DEFAULT].SetMap(GetDoorAppliedMap());
        pathFinders[(int)PathFinderType.AVOIDER].SetMap(GetDoorAndLightAppliedMap());
    }

    public int[,] GetDoorAppliedMap()
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        if (AreArraysEqual(lastDoors, doors))
        {
            if (doorAppliedMap == null)
            {
                return map;
            }
            return doorAppliedMap;
        }

        lastDoors = doors;

        ApplyDoorOnMap();

        return doorAppliedMap;
    }

    public int[,] GetDoorAndLightAppliedMap()
    {
        GetDoorAppliedMap();

        GameObject[] spotLights = GameObject.FindGameObjectsWithTag("Light");

        if (AreArraysEqual(lastSpotLights, spotLights))
        {
            if (doorAndlightAppliedMap == null)
            {
                return doorAppliedMap;
            }
            return doorAndlightAppliedMap;
        }
        lastSpotLights = spotLights;

        ApplyLightOnMap();

        return doorAndlightAppliedMap;
    }

    void ApplyDoorOnMap()
    {
        doorAppliedMap = DeepCopy(map);

        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");

        foreach (GameObject door in doors)
        {
            try
            {
                doorAppliedMap[
                    (int) door.transform.position.x - mapOffset.x,
                    (int) Math.Floor(door.transform.position.y) - mapOffset.y
                    ] = 0;
            }
            catch
            {
                continue;
            }
        }
    }

    void ApplyLightOnMap()
    {
        doorAndlightAppliedMap = DeepCopy(doorAppliedMap);

        GameObject[] spotLights = GameObject.FindGameObjectsWithTag("Light");

        foreach (GameObject spotLight in spotLights)
        {
            Light2D light = spotLight.GetComponentInChildren<Light2D>();
            if (!light.gameObject.activeSelf)
            {
                continue;
            }
            List<(int, int)> points;
            try
            {
                points = PointsInCircle(
                (int)spotLight.transform.position.x,
                (int)spotLight.transform.position.y,
                (int)light.pointLightOuterRadius);
            }
            catch
            {
                continue;
            }


            foreach ((int, int) point in points)
            {
                try
                {
                    doorAndlightAppliedMap[point.Item1 - mapOffset.x, point.Item2 - mapOffset.y] = 0;
                }
                catch { }

            }

        }
    }

    public List<(int, int)> PointsInCircle(int cx, int cy, int radius)
    {
        List<(int, int)> points = new List<(int, int)>();

        int xMin = (int)Math.Ceiling((decimal)cx - radius);
        int xMax = (int)Math.Floor((decimal)cx + radius);
        int yMin = (int)Math.Ceiling((decimal)cy - radius);
        int yMax = (int)Math.Floor((decimal)cy + radius);

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= radius * radius)
                {
                    points.Add((x, y));
                }
            }
        }

        return points;
    }


    protected int[,] DeepCopy(int[,] originalArray)
    {
        int rows = originalArray.GetLength(0);
        int cols = originalArray.GetLength(1);

        int[,] newArray = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                newArray[i, j] = originalArray[i, j];
            }
        }

        return newArray;
    }
}
