using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "stealth-master/Unit/Springs")]
public class UnitSpringSettings : SerializedScriptableObject
{
    public enum SpringType
    {
        Ground,
        Wall
    }

    [SerializeField] [DictionaryDrawerSettings(DisplayMode = DictionaryDisplayOptions.OneLine)]
    private Dictionary<(BodyState, SpringType), SpringSettings> data = new Dictionary<(BodyState, SpringType), SpringSettings>();

    public SpringSettings GetGroundSpring(BodyState state)
    {
        return data[(state, SpringType.Ground)];
    }

    public SpringSettings GetWallSpring(BodyState state)
    {
        return data[(state, SpringType.Wall)];
    }
}
