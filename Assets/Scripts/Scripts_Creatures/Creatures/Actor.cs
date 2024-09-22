using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class Actor: MonoBehaviour
{
    protected float speed = 2;
    [SerializeField] public bool debugMode = true;

    protected Vector3 deltaPosition = new Vector3();
    protected const float MOVE_FRAME = 0.05f;

    protected int maxSpeed = 4;
    protected int minSpeed = 2;

    protected PathFinderType pathFinderType = PathFinderType.DEFAULT;

    protected PathLineRenderer pathLineRenderer;

    protected Node startNode = new Node(true);
    protected Node endNode = new Node(true);

    protected ActorSoundController soundController;

    protected CreatureManager creatureManager;

    protected Vector3 direction;

    protected virtual void Awake()
    {
        soundController = GetComponent<ActorSoundController>();
    }

    protected virtual List<Node> FindPath(float x, float y)
    {
        startNode.SetPosition(transform.position.x, transform.position.y);
        endNode.SetPosition(x, y);

        try
        {
            List<Node> path = creatureManager.pathFinders[(int)pathFinderType].FindPath(startNode, endNode);
            return path;
        }
        catch (Exception e)
        {
            print(e.Message);
            return null;
        }
    }

    protected virtual void Start()
    {
        creatureManager = ReferenceManager.Instance.FindComponentByName<CreatureManager>("CreatureManager");
    }

    protected IEnumerator MoveToPosition(Node node)
    {
        float deltaX = node.X - transform.position.x;
        float deltaY = node.Y - transform.position.y;
        deltaPosition.Set(deltaX, deltaY, 0);
        deltaX = Math.Abs(deltaX);
        deltaY = Math.Abs(deltaY);

        deltaPosition.Normalize();
        direction = deltaPosition;
        while (deltaX > 0 || deltaY > 0)
        {
            deltaX -= Math.Abs(deltaPosition.x * speed * MOVE_FRAME);
            deltaY -= Math.Abs(deltaPosition.y * speed * MOVE_FRAME);
            transform.position += (deltaPosition * speed * MOVE_FRAME);
            yield return new WaitForSeconds(MOVE_FRAME);
        }
        transform.position = new Vector3(node.X, node.Y);
    }
}
