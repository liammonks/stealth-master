using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterpolateRotation : MonoBehaviour
{
    [SerializeField] private float rotationRate = 1.0f;
    private Quaternion lastRotation;

    private void Awake() {
        lastRotation = transform.rotation;
    }
    
    private void Update() {
        transform.rotation = Quaternion.Lerp(lastRotation, transform.parent.rotation, Time.deltaTime * rotationRate);
        lastRotation = transform.rotation;
    }
}
