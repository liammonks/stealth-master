using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DrinkVendor : Interactable
{
    [SerializeField] private float addThirst = 120;

    public override bool Interact(Unit interactingUnit)
    {
        if(interactingUnit is AIUnit)
        {
            (interactingUnit as AIUnit).aiStats.thirst += addThirst;
        }
        return true;
    }

}
