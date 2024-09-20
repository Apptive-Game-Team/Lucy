using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PathLineRenderer : MonoBehaviour
{
    private LineRenderer _lineRenderer;

    private Vector3[] points;

    public void SetPoints(List<Node> path)
    {
        points = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            points[i] = new Vector3(path[i].X, path[i].Y);
        }
        Render();
    }

    public void Clear()
    {
        _lineRenderer.positionCount = 0;
    }

    private void Render()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = points.Length;
        _lineRenderer.SetPositions(points);
        _lineRenderer.startWidth = 0.1f;
        _lineRenderer.endWidth = 0.1f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"))
        {
            color = Color.red
        };
    }
}
