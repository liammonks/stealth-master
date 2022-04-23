using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public enum UnitAnimation
{
    Idle,
    Run
}

[System.Serializable]
public class SpriteAnimation
{
    public UnitAnimation unitAnimation;
    public Sprite sprite;
}

public class ModelCreator
{
    [OnValueChanged("OnClassChanged")]
    public UnitClass unitClass;
    public RuntimeAnimatorController controller;

    private Dictionary<UnitClass, RuntimeAnimatorController> m_AnimatorControllerTemplates;

    public ModelCreator()
    {
        m_AnimatorControllerTemplates = new Dictionary<UnitClass, RuntimeAnimatorController>();
        m_AnimatorControllerTemplates.Add(UnitClass.Basic, FetchAnimatorController("Basic_Base"));
        m_AnimatorControllerTemplates.Add(UnitClass.Beast, FetchAnimatorController("Beast_Base"));
        m_AnimatorControllerTemplates.Add(UnitClass.Grunt, FetchAnimatorController("Grunt_Base"));
        m_AnimatorControllerTemplates.Add(UnitClass.Veteran, FetchAnimatorController("Veteran_Base"));
        m_AnimatorControllerTemplates.Add(UnitClass.Specialist, FetchAnimatorController("Specialist_Base"));
        m_AnimatorControllerTemplates.Add(UnitClass.StealthMaster, FetchAnimatorController("SM_Base"));

        unitClass = UnitClass.Basic;
    }

    private RuntimeAnimatorController FetchAnimatorController(string name)
    {
        return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"Assets/Art/2D/Animations/Characters/Templates/{name}.controller");
    }

    [Button("Add")]
    public void CreateModel()
    {
        UnitTemplate newUnit = ScriptableObject.CreateInstance<UnitTemplate>();
        AssetDatabase.CreateAsset(newUnit, "Assets/Gameplay/Units/Prefabs/New_Unit.asset");
        AssetDatabase.SaveAssets();
    }

    private void OnClassChanged()
    {
        controller = m_AnimatorControllerTemplates[unitClass];
    }
    
}
