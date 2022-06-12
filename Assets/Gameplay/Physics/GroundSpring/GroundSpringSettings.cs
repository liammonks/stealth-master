using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "stealth-master/GroundSpringSettings")]
public class GroundSpringSettings : SerializedScriptableObject
{
    public class Data
    {
        public float distance;
        public float originOffset;
        public float groundReach;
        public Vector2 size;
        public float force;
        public float damping;
        public float groundedMaxAngle;

        public Data(Data copy)
        {
            distance = copy.distance;
            originOffset = copy.originOffset;
            groundReach = copy.groundReach;
            size = copy.size;
            force = copy.force;
            damping = copy.damping;
            groundedMaxAngle = copy.groundedMaxAngle;
        }
    }

    [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.ExpandedFoldout)]
    public Dictionary<BodyState, Data> data = new Dictionary<BodyState, Data>();
}
