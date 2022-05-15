using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ModularEditor : EditorWindow
{
    [SerializeField]
    private GameObject ModularBlockPrefab;
    private static ModularEditor instance;

    [MenuItem("StealthMaster/ModularBlock")]
    public static void CreateModularBlock()
    {
        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync("ModularBlock", Selection.activeTransform);
        handle.Completed += delegate
        {
            handle.Result.transform.localPosition = Vector3.zero;
            handle.Result.transform.localRotation = Quaternion.identity;
            handle.Result.transform.localScale = Vector3.one;
            handle.Result.transform.name = "ModularBlock";
        };
        handle.WaitForCompletion();
    }

}
