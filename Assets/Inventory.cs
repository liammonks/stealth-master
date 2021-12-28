using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public static bool active = false;

    [SerializeField] private Transform[] items;
    [SerializeField] private Transform focusPoint;

    [SerializeField] private Vector2 interpolatedDelta;

    private void Update() {
        //foreach(Transform item in items)
        //{
        //    Debug.DrawLine(focusPoint.position, item.position, Color.red);
        //}
        //Debug.DrawRay(focusPoint.position, interpolatedDelta, Color.green);
    }

    private void OnMouseDelta(InputValue value)
    {
        if (!active) { return; }
        Vector2 delta = value.Get<Vector2>() * 10;
        delta.x /= Screen.width;
        delta.y /= Screen.height;
        float interpRate = (Mathf.Min(0.0f, Vector2.Dot(delta, interpolatedDelta)) * -5) + 5;
        interpolatedDelta = Vector2.MoveTowards(interpolatedDelta, delta, delta.magnitude * Time.deltaTime * interpRate);
        Vector2 bestDirection = Vector2.zero;
        float bestDirectionDot = 0.5f;
        foreach (Transform item in items)
        {
            Vector2 itemDirection = (item.position - focusPoint.position);
            Debug.DrawRay(focusPoint.position, itemDirection, Color.red);
            float itemDirectionDot = Vector2.Dot(interpolatedDelta.normalized, itemDirection.normalized);
            if(itemDirectionDot >= bestDirectionDot)
            {
                bestDirectionDot = itemDirectionDot;
                bestDirection = itemDirection;
            }
        }
        Debug.DrawRay(focusPoint.position, bestDirection, Color.blue);
        focusPoint.position += (Vector3)bestDirection * interpolatedDelta.magnitude;
    }
}
