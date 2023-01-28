#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

[ExecuteInEditMode] [RequireComponent(typeof(SceneDependencies))]
public class SceneDependenciesEditor : MonoBehaviour
{
    private bool m_Initialised = false;
    private bool m_EditingPrefab = false;

    private SceneDependencies m_SceneDependencies;
    private Coroutine m_EditPrefabDelayCoroutine;

    private void OnEnable() {
        if (m_Initialised) { return; }
        m_Initialised = true;
        m_SceneDependencies = GetComponent<SceneDependencies>();
        PrefabStage.prefabStageOpened += OnPrefabOpened;
        PrefabStage.prefabStageClosing += OnPrefabClosed;
    }
    
    private void OnDisable() {
        m_Initialised = false;
        PrefabStage.prefabStageOpened -= OnPrefabOpened;
        PrefabStage.prefabStageClosing -= OnPrefabClosed;
    }
    
    private void OnPrefabOpened(PrefabStage stage)
    {
        m_EditingPrefab = true;
    }

    private void OnPrefabClosed(PrefabStage stage)
    {
        if (m_EditPrefabDelayCoroutine != null) { StopCoroutine(m_EditPrefabDelayCoroutine); }
        m_EditPrefabDelayCoroutine = StartCoroutine(EditPrefabDelay());
        IEnumerator EditPrefabDelay()
        {
            yield return new WaitForSeconds(1.0f);
            m_EditingPrefab = false;
        }
    }

    private void Update() {
        if (EditorApplication.isPlaying || m_EditingPrefab) { return; }

        // Open scene dependencies
        foreach (KeyValuePair<string, NetworkType> kvp in m_SceneDependencies.Scenes)
        {
            if ((kvp.Value & NetworkManager.NetworkType) == 0) { continue; }

            string scenePath = GetSceneEditorPath(kvp.Key);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }

        // Close irrelevant scenes
        for (int i = 0; i < EditorSceneManager.loadedSceneCount; ++i)
        {
            Scene loadedScene = EditorSceneManager.GetSceneAt(i);
            string loadedPath = loadedScene.path;

            if (loadedScene == gameObject.scene) { continue; }

            // If scene does not contain this scene path or the network type does not match, unload scene
            if (m_SceneDependencies.Scenes.Where(x => x.Key == FromSceneEditorPath(loadedPath) && (x.Value & NetworkManager.NetworkType) != 0).Count() == 0)
            {
                EditorSceneManager.UnloadSceneAsync(loadedScene);
            }
        }
    }

    private string GetSceneEditorPath(string path)
    {
        return "Assets/" + path + ".unity";
    }

    private string FromSceneEditorPath(string path)
    {
        return path.Replace("Assets/", "").Replace(".unity", "");
    }

}

#endif