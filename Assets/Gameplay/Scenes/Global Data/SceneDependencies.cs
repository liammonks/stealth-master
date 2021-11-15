using System.Collections;
using System.Collections.Generic;
using DevLocker.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDependencies : MonoBehaviour
{
    public List<SceneReference> scenes;

    private void Awake()
    {
        foreach (SceneReference scene in scenes)
        {
            if (!SceneManager.GetSceneByName(scene.SceneName).IsValid())
            {
                SceneManager.LoadScene(scene.SceneName, LoadSceneMode.Additive);
            }
        }
    }
}
