using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedRigidbody : MonoBehaviour
{
    public Vector2 velocity;

    private Vector2 lastPosition;
    
    private void Awake() {
        lastPosition = transform.position;
    }
    
    private void FixedUpdate() {
        velocity = ((Vector2)transform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }
}
