using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Actor
{
   int[,] grid = new int[,]
   {
        { 0, 1, 0, 0, 0, 0, 0 },
        { 0, 1, 0, 1, 1, 1, 0 },
        { 0, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 0, 1, 0 },
        { 0, 1, 0, 1, 0, 1, 0 },
        { 0, 1, 0, 0, 0, 0, 0 },
        { 0, 1, 0, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 0, 0, 0 }
   };

    Node startNode = new Node(0, 0, true);
    Node endNode = new Node(6, 6, true);

    void Start()
    {
        PathFinder pathFinder = new PathFinder(grid);
        try
        {
            List<Node> path = pathFinder.FindPath(startNode, endNode);
            print(path);
            foreach (Node node in path)
            {
                Console.WriteLine($"X: {node.X}, Y: {node.Y}");
            }
        }
        catch(Exception e)
        {
            print(e.Message);
        }
    }

    
    void Update()
    {
        
    }
}
