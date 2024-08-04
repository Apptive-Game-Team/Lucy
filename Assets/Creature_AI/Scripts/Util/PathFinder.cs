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
    public int X { get; }
    public int Y { get; }
    public bool IsWalkable { get; }
    public int GCost;
    public int HCost;
    public Node Parent;

    public int FCost => GCost + HCost;

    public Node(int x, int y, bool isWalkable)
    {
        X = x;
        Y = y;
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
    private readonly int[,] grid;
    private readonly int width;
    private readonly int height;
    private Node[,] nodes;
    public PathFinder(int[,] grid)
    {
        this.grid = grid;
        
        width = grid.GetLength(0);
        height = grid.GetLength(1);
        nodes = new Node[width, height];
    }

    public List<Node> FindPath(Node startNode, Node endNode)
    {
        PriorityQueue<Node, int> openQueue = new PriorityQueue<Node, int>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, endNode);
        openQueue.Enqueue(startNode, startNode.FCost);

        while (openQueue.Count > 0)
        {
            Node currentNode = openQueue.Dequeue();
            closedSet.Add(currentNode);

            if (currentNode.Equals(endNode)) // path found
            {
                Debug.Log("Path Found");
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.GCost || !openQueue.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;

                    if (!openQueue.Contains(neighbor))
                    {
                        openQueue.Enqueue(neighbor, neighbor.FCost);
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
                        tempNode = new Node(checkX, checkY, grid[checkX, checkY] == 0);
                        
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
        int[,] markedGrid = (int[,])grid.Clone();
        markedGrid[x, y] = 2;

        PrintGrid(markedGrid);
    }

    private void PrintGrid(int[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            StringBuilder gridString = new StringBuilder();
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                gridString.Append(grid[i, j] + " ");
            }
            Debug.Log(gridString.ToString());
        }
    }
}