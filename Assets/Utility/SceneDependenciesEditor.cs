#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ParrelSync;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode] [RequireComponent(typeof(SceneDependencies))]
public class SceneDependenciesEditor : MonoBehaviour
{
    private bool m_Initialised = false;
    private bool m_EditingPrefab = false;
    private Coroutine m_EditPrefabDelayCoroutine;

    private void OnEnable() {
        if (m_Initialised) { return; }
        m_Initialised = true;
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

        foreach (KeyValuePair<string, NetworkType> kvp in GetComponent<SceneDependencies>().Scenes)
        {
            if (kvp.Value != NetworkType.Both && kvp.Value != NetworkManager.NetworkType) { continue; }
            string scenePath = "Assets/" + kvp.Key + ".unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
        }
    }
}

#endif