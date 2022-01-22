using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkUnit : Unit
{
    public override void Die()
    {
        data.rb.velocity = Vector2.zero;
        transform.position = LevelManager.Instance.ActivePlayerSpawn.transform.position;
    }
}
