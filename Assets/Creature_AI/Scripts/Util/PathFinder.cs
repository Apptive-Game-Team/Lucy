using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

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
    private readonly int[,] map;
    private readonly int width;
    private readonly int height;
    private Node[,] nodes;
    public PathFinder(int[,] map)
    {
        this.map = map;
        
        width = map.GetLength(0);
        height = map.GetLength(1);
        nodes = new Node[width, height];
    }

    public List<Node> FindPath(Node startNode, Node endNode)
    {
        SortedSet<Node> openSet = new SortedSet<Node>() { startNode};
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, endNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Min;
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.Equals(endNode)) // path found
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
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

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = node.X + x;
                int checkY = node.Y + y;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    Node tempNode;
                    if (this.nodes[checkX, checkY] == null)
                    {
                        tempNode = new Node(checkX, checkY, map[checkX, checkY] == 0);
                        
                        this.nodes[checkX, checkY] = tempNode;
                    }
                    neighbors.Add(this.nodes[checkX, checkY]);
                }
            }
        }
        return neighbors;
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
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }

    public void MarkPosition(int x, int y)
    {
        int[,] markedmap = (int[,])map.Clone();
        markedmap[x, y] = 2;

        Printmap(markedmap);
    }

    private void Printmap(int[,] map)
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            StringBuilder mapString = new StringBuilder();
            for (int j = 0; j < map.GetLength(1); j++)
            {
                mapString.Append(map[i, j] + " ");
            }
            Debug.Log(mapString.ToString());
        }
    }
}