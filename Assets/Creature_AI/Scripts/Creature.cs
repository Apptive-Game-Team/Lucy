using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Creature : Actor
{
    PathLineRenderer pathLineRenderer;
    Player player;
    int[,] map;
    Vector3 deltaPosition = new Vector3();
    Node startNode = new Node(true);
    Node endNode = new Node(true);
    float moveFrame = 0.05f;

    void Start()
    {
        player = CreatureManager.Instance.GetPlayer();
        map = CreatureManager.Instance.GetMap();
        pathLineRenderer = GetComponent<PathLineRenderer>();
        StartCoroutine(Schedule());
    }

    IEnumerator Schedule()
    {
        yield return new WaitForSeconds(5);
        temp();  
    }

    void temp()
    {
        StopAllCoroutines();
        List<Node> path = FindPath(player.transform.position.x, player.transform.position.y);
        if (path != null)
        {
            StartCoroutine(MoveOnPath(path));
        }
        StartCoroutine(Schedule());
    }
    
    private List<Node> FindPath(float x, float y)
    {
        startNode.SetPosition(transform.position.x, transform.position.y);
        endNode.SetPosition(x, y);
        PathFinder pathFinder = new PathFinder(map);
        try
        {
            List<Node> path = pathFinder.FindPath(startNode, endNode);
            pathLineRenderer.SetPoints(path);
            return path;
        }
        catch (Exception e)
        {
            print(e.Message);
            return null;
        }
    }

    
    IEnumerator MoveOnPath(List<Node> path)
    {
        foreach(Node node in path)
        {
            float deltaX = node.X - transform.position.x;
            float deltaY = node.Y - transform.position.y;
            deltaPosition.Set(deltaX, deltaY, 0);
            deltaPosition.Normalize();
            
            while (Math.Abs(deltaX) > 0.1 && Math.Abs(deltaY) > 0.1){
                transform.position += (deltaPosition * speed * moveFrame);
                yield return new WaitForSeconds(moveFrame);
            }
            transform.position = new Vector3(node.X, node.Y);
        }
    }
}
