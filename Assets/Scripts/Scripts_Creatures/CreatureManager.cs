using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.Rendering.Universal;
using static Team6203.Util;

/// <summary>
/// Types of pathfinders used by different creature behaviors.
/// </summary>
public enum PathFinderType
{
    DEFAULT=0,  // Standard pathfinding - considers walls and doors
    AVOIDER=1,  // Avoider pathfinding - also avoids light sources
}

/// <summary>
/// Manages all creatures in the scene and provides pathfinding services.
/// Builds and maintains map data from tilemaps, applying doors and lights as obstacles.
/// Provides different pathfinder instances for different creature types.
/// </summary>
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

    private void OnEnable()
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

    /// <summary>
    /// Returns a map with doors marked as walkable (0).
    /// Caches result and only recalculates when doors change.
    /// Used by DEFAULT pathfinder type.
    /// </summary>
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

    /// <summary>
    /// Returns a map with both doors and active lights marked as walkable/unwalkable.
    /// Light areas are marked as obstacles for AVOIDER pathfinder.
    /// Caches result and only recalculates when doors or lights change.
    /// </summary>
    public int[,] GetDoorAndLightAppliedMap()
    {
        GetDoorAppliedMap();

        GameObject[] spotLights = GameObject.FindGameObjectsWithTag("Light");

        if (AreArraysEqual(lastSpotLights, spotLights))
        {
            if (doorAndlightAppliedMap == null)
            {
                if (doorAppliedMap == null)
                {
                    return map;
                }
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
            int x = (int)door.transform.position.x - mapOffset.x;
            int y = (int)Math.Floor(door.transform.position.y) - mapOffset.y;
            
            if (x >= 0 && x < doorAppliedMap.GetLength(0) && y >= 0 && y < doorAppliedMap.GetLength(1))
            {
                doorAppliedMap[x, y] = 0;
            }
            else
            {
                Debug.LogWarning($"Door position ({x}, {y}) is out of map bounds");
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
            if (light == null || !light.gameObject.activeSelf)
            {
                continue;
            }
            
            List<(int, int)> points = PointsInCircle(
                (int)spotLight.transform.position.x,
                (int)spotLight.transform.position.y,
                (int)light.pointLightOuterRadius);

            foreach ((int, int) point in points)
            {
                int x = point.Item1 - mapOffset.x;
                int y = point.Item2 - mapOffset.y;
                
                if (x >= 0 && x < doorAndlightAppliedMap.GetLength(0) && y >= 0 && y < doorAndlightAppliedMap.GetLength(1))
                {
                    doorAndlightAppliedMap[x, y] = 0;
                }
            }
        }
    }

    /// <summary>
    /// Calculates all integer coordinate points within a circular radius.
    /// Used to determine which map cells are affected by a light source.
    /// Uses the circle equation: (x-cx)² + (y-cy)² ≤ r²
    /// </summary>
    /// <param name="cx">Center x coordinate</param>
    /// <param name="cy">Center y coordinate</param>
    /// <param name="radius">Radius of the circle</param>
    /// <returns>List of (x, y) coordinates within the circle</returns>
    public List<(int, int)> PointsInCircle(int cx, int cy, int radius)
    {
        List<(int, int)> points = new List<(int, int)>();

        int xMin = (int)Math.Ceiling((decimal)cx - radius);
        int xMax = (int)Math.Floor((decimal)cx + radius);
        int yMin = (int)Math.Ceiling((decimal)cy - radius);
        int yMax = (int)Math.Floor((decimal)cx + radius);

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
