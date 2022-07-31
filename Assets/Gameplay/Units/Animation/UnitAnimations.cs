using UnityEngine;

[CreateAssetMenu(menuName = "stealth-master/Unit/Animations")]
public class UnitAnimations : ScriptableObject
{
    public UnitLayerControllers Body;
    public UnitArmAnimations Arm;
}

[System.Serializable]
public class UnitLayerControllers
{
    public RuntimeAnimatorController Default;
    public RuntimeAnimatorController Reversed;
}

[System.Serializable]
public class UnitArmAnimations
{
    public UnitLayerControllers Unarmed;
    public UnitLayerControllers Pistol;
    public UnitLayerControllers GrapplingHook;
}