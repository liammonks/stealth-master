using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MLRoute : MonoBehaviour
{
    [Header("Start")]
    [SerializeField] private Transform startTransform;
    [SerializeField] private float startRange;

    [Header("End")]
    [SerializeField] private Transform endTransform;
    [SerializeField] private float endRange;
    
    public Vector2 GetStart()
    {
        Vector2 offset = new Vector2(Random.Range(-startRange / 2, startRange / 2), 0);
        return (Vector2)startTransform.position + offset;
    }

    public Vector2 GetEnd()
    {
        Vector2 offset = new Vector2(Random.Range(-endRange / 2, endRange / 2), 0);
        return (Vector2)endTransform.position + offset;
    }
    
    private void Update() {
        #if UNITY_EDITOR
        Vector2 startMin = (Vector2)startTransform.position + new Vector2(-startRange / 2, 0);
        Vector2 startMax = (Vector2)startTransform.position + new Vector2(startRange / 2, 0);
        Debug.DrawLine(startMin, startMax, Color.green);
        
        Vector2 endMin = (Vector2)endTransform.position + new Vector2(-endRange / 2, 0);
        Vector2 endMax = (Vector2)endTransform.position + new Vector2(endRange / 2, 0);
        Debug.DrawLine(endMin, endMax, Color.red);
        
        Debug.DrawLine(startTransform.position, endTransform.position, Color.blue);
        #endif
    }
}
