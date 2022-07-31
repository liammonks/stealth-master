using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "stealth-master/Physics/Spring")]
public class SpringSettings : SerializedScriptableObject
{
    public Vector2 position;
    public Vector2 direction;
    public Vector2 size;
    public float restDistance;
    public float reachDistance;
    public float force;
    public float damping;
    public float slipAngle;

    public SpringSettings Clone()
    {
        SpringSettings clone = ScriptableObject.CreateInstance<SpringSettings>();
        clone.position = position;
        clone.direction = direction;
        clone.size = size;
        clone.restDistance = restDistance;
        clone.reachDistance = reachDistance;
        clone.force = force;
        clone.damping = damping;
        clone.slipAngle = slipAngle;
        return clone;
    }
}
