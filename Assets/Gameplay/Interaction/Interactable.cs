using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract bool Interact(Unit interactingUnit);
    
    private void OnTriggerEnter2D(Collider2D other) {
        Unit unit = other.attachedRigidbody.GetComponent<Unit>();
        if (unit != null)
        {
            unit.AddInteractable(this);
            if (unit is Player)
            {
                PlayerUI.Instance.EnableInteractPopup(transform);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other) {
        Unit unit = other.attachedRigidbody.GetComponent<Unit>();
        if (unit != null)
        {
            unit.RemoveInteractable(this);
            if (unit is Player)
            {
                PlayerUI.Instance.DisableInteractPopup();
            }
        }
    }
}
