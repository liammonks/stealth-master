using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vent : Interactable
{
    [SerializeField] private Vent leftVent, rightVent, upVent, downVent;

    public override void OnInteract(Unit_OLD interactingUnit)
    {
        // Only the player can enter a vent
        if (interactingUnit is Player_OLD)
        {
            Player_OLD player = (interactingUnit as Player_OLD);
            if(player.GetCurrentVent() == this)
            {
                // Player already in this vent, exit instead
                player.ExitVent();
            }
            else
            {
                player.EnterVent(this);
            }
        }
    }
    
    public Vent GetConnectedVent(Vector2 direction)
    {
        if (direction == Vector2.left)
        {
            return leftVent;
        }
        if (direction == Vector2.right)
        {
            return rightVent;
        }
        if (direction == Vector2.up)
        {
            return upVent;
        }
        if(direction == Vector2.down)
        {
            return downVent;
        }
        return null;
    }
}
