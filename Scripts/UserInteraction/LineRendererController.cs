using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    private LineRenderer lineRenderer;

    public float width = 0.1f;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.enabled = false;
    }

    public void SetPoints(Vector3 a, Vector3 b)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, a);
        lineRenderer.SetPosition(1, b);
    }

    public void Activate(){
        lineRenderer.enabled = true;

    }
    public void Deactivate(){
        lineRenderer.enabled = false;
        
    }
}
