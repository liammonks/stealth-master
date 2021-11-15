#if UNITY_EDITOR

using System.Collections.Generic;
using DevLocker.Utils;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[ExecuteInEditMode] [RequireComponent(typeof(SceneDependencies))]
public class SceneDependenciesEditor : MonoBehaviour
{
    private void Awake()
    {
        foreach (SceneReference scene in gameObject.GetComponent<SceneDependencies>().scenes)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene(scene.ScenePath, OpenSceneMode.Additive);
            }
        }
    }
}

#endif