using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        Unit unit = other.attachedRigidbody.GetComponent<Unit>();
        if(unit != null)
        {
            unit.Die();
        }
    }
}
