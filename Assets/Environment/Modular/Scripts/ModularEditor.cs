using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ModularEditor : EditorWindow
{
    [MenuItem("StealthMaster/ModularBlock")]
    public static void CreateModularBlock()
    {
        GameObject modularBlock = new GameObject("ModularBlock", typeof(ModularBlock));
        modularBlock.transform.SetParent(Selection.activeTransform);
        modularBlock.transform.localPosition = Vector3.zero;
        modularBlock.transform.localRotation = Quaternion.identity;
        modularBlock.transform.localScale = Vector3.one;
    }
}
