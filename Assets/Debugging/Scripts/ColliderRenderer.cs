using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderRenderer : MonoBehaviour
{
    [SerializeField]
    private Material m_Material;

    private List<LineRenderer> m_LineRenderers = new List<LineRenderer>();
    private Transform m_LineRenderersParent;
    private Coroutine m_EndRenderingCoroutine;
    private int m_RenderIndex = 0;

    private void Awake()
    {
        m_LineRenderersParent = new GameObject("LineRenderers").transform;
        m_LineRenderersParent.SetParent(transform);
    }

    public void DrawCollider(Collider2D collider, Color color)
    {
        if (m_RenderIndex > m_LineRenderers.Count - 1)
        {
            AddLineRenderer();
        }

        LineRenderer lineRenderer = m_LineRenderers[m_RenderIndex];
        lineRenderer.enabled = true;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        if (collider is BoxCollider2D) { DrawBoxCollider(collider as BoxCollider2D, lineRenderer); }

        m_RenderIndex++;
        if (m_EndRenderingCoroutine == null)
        {
            m_EndRenderingCoroutine = StartCoroutine(EndRendering());
        }
    }

    private void AddLineRenderer()
    {
        GameObject lineRendererObject = new GameObject($"LineRenderer [{m_LineRenderers.Count}]");
        lineRendererObject.transform.SetParent(m_LineRenderersParent);
        lineRendererObject.layer = LayerMask.NameToLayer("Overlay");

        LineRenderer lineRenderer = lineRendererObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.025f;
        lineRenderer.material = m_Material;
        m_LineRenderers.Add(lineRenderer);

    }

    private IEnumerator EndRendering()
    {
        yield return new WaitForEndOfFrame();
        foreach (LineRenderer lineRenderer in m_LineRenderers)
        {
            lineRenderer.enabled = false;
        }
        m_EndRenderingCoroutine = null;
        m_RenderIndex = 0;
    }

    private void DrawBoxCollider(BoxCollider2D boxCollider2D, LineRenderer lineRenderer)
    {
        Vector3[] positions = new Vector3[4];
        lineRenderer.positionCount = 4;
        positions[0] = boxCollider2D.transform.TransformPoint(new Vector3(boxCollider2D.size.x / 2.0f, boxCollider2D.size.y / 2.0f, 0));
        positions[1] = boxCollider2D.transform.TransformPoint(new Vector3(-boxCollider2D.size.x / 2.0f, boxCollider2D.size.y / 2.0f, 0));
        positions[2] = boxCollider2D.transform.TransformPoint(new Vector3(-boxCollider2D.size.x / 2.0f, -boxCollider2D.size.y / 2.0f, 0));
        positions[3] = boxCollider2D.transform.TransformPoint(new Vector3(boxCollider2D.size.x / 2.0f, -boxCollider2D.size.y / 2.0f, 0));
        lineRenderer.SetPositions(positions);
    }
}
