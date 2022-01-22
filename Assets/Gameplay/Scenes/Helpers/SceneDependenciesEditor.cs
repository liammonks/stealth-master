using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

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
        #if UNITY_EDITOR
        if (editingPrefab) { return; }
        foreach (string scene in gameObject.GetComponent<SceneDependencies>().scenes)
        {
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene("Assets/Gameplay/Scenes/" + scene + ".unity", OpenSceneMode.Additive);
            }
        }
        #endif
    }
}
