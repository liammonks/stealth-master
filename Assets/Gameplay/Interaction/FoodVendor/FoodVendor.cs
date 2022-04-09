using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FoodVendor : Interactable
{

    public override bool Interact(Unit interactingUnit)
    {
        return true;
    }

}
