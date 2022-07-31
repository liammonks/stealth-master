#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[ExecuteInEditMode] [RequireComponent(typeof(SceneDependencies))]
public class SceneDependenciesEditor : MonoBehaviour
{
    private bool initialised = false;
    private bool editingPrefab = false;
    
    private void OnEnable() {
        if (initialised) { return; }
        initialised = true;
        PrefabStage.prefabStageOpened += OnPrefabOpened;
        PrefabStage.prefabStageClosing += OnPrefabClosed;
    }
    
    private void OnDisable() {
        initialised = false;
        PrefabStage.prefabStageOpened -= OnPrefabOpened;
        PrefabStage.prefabStageClosing -= OnPrefabClosed;
    }
    
    private void OnPrefabOpened(PrefabStage stage)
    {
        editingPrefab = true;
    }

    private void OnPrefabClosed(PrefabStage stage)
    {
        editingPrefab = false;
    }
    
    private void Update() {
        if (editingPrefab) { return; }
        foreach (string scene in gameObject.GetComponent<SceneDependencies>().scenes)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene(scene + ".unity", OpenSceneMode.Additive);
            }
        }
    }
}

#endif