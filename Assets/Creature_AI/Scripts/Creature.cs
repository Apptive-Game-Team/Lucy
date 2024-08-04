using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : Actor
{
    [SerializeField] Player player;
    int[,] grid = new int[,]
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

    Node startNode = new Node(1, 1, true);
    Node endNode = new Node(7, 7, true);

    void Start()
    {
        StartCoroutine(MoveToP());
    }

    IEnumerator MoveToP()
    {
        yield return new WaitForSeconds(5);
        temp();  
    }
    void temp()
    {
        StopAllCoroutines();
        MoveToPlayer();
        StartCoroutine(MoveToP());
    }
    

    void MoveToPlayer()
    {
        startNode = new Node((int)transform.position.x+8, (int)transform.position.y+4, true);
        endNode = new Node((int)player.transform.position.x + 8, (int)player.transform.position.y+4, true);
        PathFinder pathFinder = new PathFinder(grid);
        try
        {
            List<Node> path = pathFinder.FindPath(startNode, endNode);
            StartCoroutine(MoveOnPath(path));
        }
        catch (Exception e)
        {
            print(e.Message);
        }
    }

    
    IEnumerator MoveOnPath(List<Node> path)
    {
        foreach(Node node in path)
        {
            yield return new WaitForSeconds(1f);
            transform.position = new Vector3(node.X - 8, node.Y-4);
        }
    }
}
