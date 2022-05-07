using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DrinkVendor : Interactable
{
    [SerializeField] private float addThirst = 120;

    public override bool Interact(Unit interactingUnit)
    {
        return true;
    }

}
