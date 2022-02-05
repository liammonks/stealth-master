using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FoodVendor : Interactable
{
    [SerializeField] private float addHunger = 80;

    public override bool Interact(Unit interactingUnit)
    {
        if(interactingUnit is Enemy)
        {
            (interactingUnit as Enemy).aiStats.hunger += addHunger;
        }
        return true;
    }

}
