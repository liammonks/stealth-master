using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

public class SceneDependencies : SerializedMonoBehaviour
{

    public Dictionary<string, NetworkType> Scenes => m_Scenes;

    [SerializeField]
    private Dictionary<string, NetworkType> m_Scenes;

    private void Awake()
    {
        foreach (KeyValuePair<string, NetworkType> kvp in m_Scenes)
        {
            if (kvp.Value != NetworkType.Both && kvp.Value != NetworkManager.NetworkType) { continue; }

            if (!SceneManager.GetSceneByName(kvp.Key).IsValid())
            {
                SceneManager.LoadScene(kvp.Key, LoadSceneMode.Additive);
            }
        }
    }
}
