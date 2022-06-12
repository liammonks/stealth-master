using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColliderInfo
{
    public float Width => m_Width;
    public float Height => m_Height;

    private float m_Width;
    private float m_Height;

    public UnitColliderInfo(Collider2D[] colliders)
    {
        Vector2 min = Vector2.zero;
        Vector2 max = Vector2.zero;
        foreach (Collider2D collider in colliders)
        {
            if (collider is BoxCollider2D)
            {
                BoxCollider2D boxCollider = (BoxCollider2D)collider;
                min.x = Mathf.Min(min.x, boxCollider.transform.localPosition.x - (boxCollider.size.x * 0.5f));
                min.y = Mathf.Min(min.y, boxCollider.transform.localPosition.y - (boxCollider.size.y * 0.5f));
                max.x = Mathf.Max(max.x, boxCollider.transform.localPosition.x + (boxCollider.size.x * 0.5f));
                max.y = Mathf.Max(max.y, boxCollider.transform.localPosition.y + (boxCollider.size.y * 0.5f));
            }
            if (collider is CircleCollider2D)
            {
                CircleCollider2D circleCollider = (CircleCollider2D)collider;
                min.x = Mathf.Min(min.x, circleCollider.transform.localPosition.x - circleCollider.radius);
                min.y = Mathf.Min(min.y, circleCollider.transform.localPosition.y - circleCollider.radius);
                max.x = Mathf.Max(max.x, circleCollider.transform.localPosition.x + circleCollider.radius);
                max.y = Mathf.Max(max.y, circleCollider.transform.localPosition.y + circleCollider.radius);
            }
        }
        m_Width = max.x - min.x;
        m_Height = max.y - min.y;
    }
}
