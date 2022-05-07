using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

public enum UnitClass
{
    Basic,
    Beast,
    Grunt,
    Veteran,
    Specialist,
    StealthMaster
}

public class UnitCreator
{
    public string name = "New_Unit";
    public UnitClass unitClass = UnitClass.Basic;

    public UnitCreator()
    {
        
    }

    [Button("Add")]
    public void CreateUnit()
    {
        UnitTemplate newUnit = ScriptableObject.CreateInstance<UnitTemplate>();
        newUnit.unitClass = unitClass;

        AssetDatabase.CreateAsset(newUnit, $"Assets/Gameplay/Units/Prefabs/{name}.asset");
        AssetDatabase.SaveAssets();
    }
}
