using System.Collections.Generic;
using UnityEngine;

namespace Scripts_Creatures.Util
{
    public class PathLineRenderer : MonoBehaviour
    {
        [SerializeField, Tooltip("Show AI navigation paths for debugging only.")]
        private bool showDebugPath = false;
        private LineRenderer _lineRenderer;
        private Material debugMaterial;

        private Vector3[] points;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
                _lineRenderer.enabled = showDebugPath;
            }
        }

        public void SetPoints(List<Node> path)
        {
            if (_lineRenderer == null) return;
            _lineRenderer.enabled = showDebugPath;
            if (!showDebugPath || path == null)
            {
                Clear();
                return;
            }
            points = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                points[i] = new Vector3(path[i].X, path[i].Y);
            }
            Render();
        }

        public void Clear()
        {
            if (_lineRenderer != null) _lineRenderer.positionCount = 0;
        }

        private void Render()
        {
            _lineRenderer.positionCount = points.Length;
            _lineRenderer.SetPositions(points);
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.1f;
            if (debugMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader == null) { Clear(); return; }
                debugMaterial = new Material(shader) { color = Color.red };
                _lineRenderer.sharedMaterial = debugMaterial;
            }
        }

        private void OnDisable()
        {
            Clear();
            if (_lineRenderer != null) _lineRenderer.enabled = false;
        }

        private void OnDestroy()
        {
            if (debugMaterial != null) Destroy(debugMaterial);
        }
    }
}
