using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private string targetID;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.attachedRigidbody?.GetComponent<Player>())
        {
            GlobalEvents.TargetReached(targetID);
        }
    }
}
