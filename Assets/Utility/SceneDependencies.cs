using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using System.Linq;

public class SceneDependencies : SerializedMonoBehaviour
{

    public IEnumerable<KeyValuePair<string, NetworkType>> Scenes => m_Scenes.Where(x => (x.Value & NetworkManager.NetworkType) != 0);

    [SerializeField]
    [DictionaryDrawerSettings(KeyLabel = "Scene Path", ValueLabel = "Network Condition")]
    private Dictionary<string, NetworkType> m_Scenes;

    private void Awake()
    {
        foreach (KeyValuePair<string, NetworkType> kvp in m_Scenes)
        {
            if ((kvp.Value & NetworkManager.NetworkType) == 0) { continue; }

            if (!SceneManager.GetSceneByName(kvp.Key).IsValid())
            {
                SceneManager.LoadScene(kvp.Key, LoadSceneMode.Additive);
            }
        }
    }
}
