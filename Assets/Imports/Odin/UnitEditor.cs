using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class UnitEditor : OdinMenuEditorWindow
{
    [MenuItem("Tools/Unit Editor")]
    private static void OpenWindow()
    {
        GetWindow<UnitEditor>().Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        OdinMenuTree tree = new OdinMenuTree();
        tree.Add("Create/Unit", new UnitCreator());
        tree.Add("Create/Model", new ModelCreator());
        tree.Add("Create/Gadget", new UnitCreator());

        tree.AddAllAssetsAtPath("Units", "Assets/Gameplay/Units/Prefabs", typeof(GameObject));
        return tree;
    }

}
