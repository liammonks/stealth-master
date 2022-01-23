using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AnimatedRigidbody : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 lastPosition;
    
    private void Awake() {
        lastPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void LateUpdate() {
        rb.velocity = ((Vector2)transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }
}
