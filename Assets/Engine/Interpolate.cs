using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolate : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float rate = 5.0f;
    [SerializeField] private float breakDistance = 0.1f;
    [SerializeField] private float connectDistance = 0.05f;
    
    private bool interpolating = false;

    private void Update() {
        float dist = Vector2.Distance(transform.position, targetTransform.position + targetOffset);
        if (dist >= breakDistance)
        {
            interpolating = true;
        }
        else if (dist < connectDistance)
        {
            interpolating = false;
        }

        if (interpolating)
        {
            transform.position = Vector3.Lerp(transform.position, targetTransform.position + targetOffset, Time.deltaTime * rate);
        }
        else
        {
            transform.position = targetTransform.position + targetOffset;
        }

        transform.rotation = targetTransform.rotation;
    }
}
