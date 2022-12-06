using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class PrefabVisualiser : MonoBehaviour
{
    [SerializeField]
    private GameObject m_Prefab;

    private GameObject m_LoadedPrefab;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            Destroy(m_LoadedPrefab);
        }
    }

    private void OnValidate()
    {
        if (m_Prefab == null || Application.isPlaying) { return; }
        StartCoroutine(LoadPrefab());
    }

    private IEnumerator LoadPrefab()
    {
        yield return new WaitForEndOfFrame();
        if (m_LoadedPrefab != null) { DestroyImmediate(m_LoadedPrefab); }

        m_LoadedPrefab = Instantiate(m_Prefab, transform);
        m_LoadedPrefab.transform.localPosition = Vector3.zero;
        m_LoadedPrefab.transform.localRotation = Quaternion.identity;
        m_LoadedPrefab.hideFlags = HideFlags.HideAndDontSave;
    }
}
#endif