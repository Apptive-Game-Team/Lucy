using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Node : IComparable<Node>
{
    public int X;
    public int Y;
    public bool IsWalkable;
    public int GCost;
    public int HCost;
    public Node Parent;

    public int FCost => GCost + HCost;

    public void SetPosition(float x, float y)
    {
        X = (int)Math.Round(x); Y = (int)Math.Round(y);
    }

    public Node(int x, int y, bool isWalkable)
    {
        X = x;
        Y = y;
        IsWalkable = isWalkable;
    }

    public Node(bool isWalkable)
    {
        IsWalkable = isWalkable;
    }

    public int CompareTo(Node other)
    {
        int compare = FCost.CompareTo(other.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(other.HCost);
        }
        return compare;
    }

    public override bool Equals(object obj)
    {
        if (obj is Node other)
        {
            return X == other.X && Y == other.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (X, Y).GetHashCode();
    }
}

public class PathFinder
{
    private int[,] map;
    private readonly int width;
    private readonly int height;
    private Vector3Int mapOffset;
    private List<Tuple<int, int>> directions;
    public int maxComputing = 1000;

    private bool debugMode = false;

    private Node[,] nodes;
    public PathFinder(int[,] map, Vector3Int mapOffset, bool debugMode = false)
    {
        this.map = map;
        this.mapOffset = mapOffset;
        width = map.GetLength(0);
        height = map.GetLength(1);
        nodes = new Node[width, height];
        InitDirections();
    }

    public void SetMap(int[,] newMap)
    {
#if UNITY_EDITOR
        if (debugMode)
        {
            Debug.Log("Map Updated");
        }

#endif
        this.map = newMap;
        nodes = new Node[width, height];
    }

    public List<Node> FindPath(Node startNode, Node endNode)
    {
        int computeCount = 0;

        SortedSet<Node> openSet = new SortedSet<Node>() { startNode};
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, endNode);

        while (openSet.Count > 0 && computeCount < maxComputing)
        {
            computeCount++;
            Node currentNode = openSet.Min;
            if (!currentNode.IsWalkable)
            {
                throw new Exception("creature not available");
            }

            openSet.Remove(openSet.Min);
            closedSet.Add(currentNode);

            if (currentNode.Equals(endNode)) // path found
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (neighbor == null || !neighbor.IsWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }
                int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        throw new Exception("Path not Found");
    }


    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        foreach (Tuple<int, int> direction in directions) { 
            int checkX = node.X + direction.Item1;
            int checkY = node.Y + direction.Item2;

            if (checkX - mapOffset.x >= 0 && checkX - mapOffset.x < width && checkY - mapOffset.y >= 0 && checkY - mapOffset.y < height)
            {
                Node tempNode = GetNode(checkX, checkY);
                if (tempNode.IsWalkable)
                {
                    neighbors.Add(tempNode);
                }
                
            }
        }
        return neighbors;
    }

    private Node GetNode(int x, int y)
    {
        Node tempNode;
        if (this.nodes[x - mapOffset.x, y - mapOffset.y] == null)
        {
            tempNode = new Node(x, y, map[x - mapOffset.x, y - mapOffset.y] == 1);

            this.nodes[x - mapOffset.x, y - mapOffset.y] = tempNode;
        }
        tempNode = this.nodes[x - mapOffset.x, y - mapOffset.y];

        return tempNode;
    }

    public List<Node> FindDirectionPath(Node startNode, Vector3 direction, float maxDistance = 10f)
    {
        SortedSet<Node> openSet = new SortedSet<Node>() { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();


        int computingCounter = 0;

        while (openSet.Count > 0 && computingCounter < maxComputing)
        {
            computingCounter++;
            Node currentNode = openSet.Min;
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            if (GetRealDistance(startNode, currentNode) > maxDistance)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float gCost = currentNode.GCost + GetDistance(currentNode, neighbor);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                } else if (gCost >= neighbor.GCost)
                {
                    continue;
                }

                neighbor.GCost = (int)gCost;
                neighbor.HCost = (int) Vector3.Angle(direction, (new Vector3(neighbor.X - startNode.X, neighbor.Y - startNode.Y)).normalized);
                neighbor.Parent = currentNode;
            }
        }

        return null;
    }

    public List<Node> FindRandomPath(Node startNode, Vector3 direction , int nodeLen)
    {
        List<Node> path = new List<Node>();

        int computingCounter = 0;

        Node lastNode = startNode;

        List<Node> neighbors = GetNeighbors(startNode);
        System.Random random = new System.Random();
        int currentDirection = directions.FindIndex(t => t.Item1 == Mathf.Sign(direction.x) && t.Item2 == Mathf.Sign(direction.y));
        for (int i = 0; i < nodeLen; i++)
        {
            computingCounter++;
            if (computingCounter > maxComputing)
            {
#if UNITY_EDITOR
                if (debugMode)
                    Debug.Log("RandomPath Not Found");
#endif
                return null;
            }
            int tempRandomNum = random.Next(0, 5);

            if (tempRandomNum == 1)
            {
                currentDirection++;
                if (currentDirection > 3)
                    currentDirection = 0;
            }
            else if (tempRandomNum == 2)
            {
                currentDirection--;
            }
            if (currentDirection < 0)
                currentDirection = 3;

            Node node = GetNode(
                directions[currentDirection].Item1 + lastNode.X,
                directions[currentDirection].Item2 + lastNode.Y
                );
            if (!node.IsWalkable)
            {
                i--;
                continue;
            }
            lastNode = node;
            path.Add(node);
        }

        return path;
        
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Math.Abs(nodeA.X - nodeB.X);
        int dstY = Math.Abs(nodeA.Y - nodeB.Y);
        return 10 * (dstX + dstY);
    }

    private void InitDirections()
    {
        directions = new List<Tuple<int, int>>{
            Tuple.Create(1, 0),
            Tuple.Create(0, -1),
            Tuple.Create(-1, 0),
            Tuple.Create(0, 1)
        };

    }

    private float GetRealDistance(Node node1, Node node2)
    {
        float deltaX = node1.X - node2.X;
        float deltaY = node1.Y - node2.Y;
        return (float) Math.Pow(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2), 0.5);
    }
}