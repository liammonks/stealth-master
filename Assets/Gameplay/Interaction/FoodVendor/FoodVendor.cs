using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FoodVendor : Interactable
{
    [SerializeField] private float addHunger = 80;

    public override bool Interact(Unit interactingUnit)
    {
        if(interactingUnit is AIUnit)
        {
            (interactingUnit as AIUnit).aiStats.hunger += addHunger;
        }
        return true;
    }

}
