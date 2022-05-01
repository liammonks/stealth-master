using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDependencies : MonoBehaviour
{
    public List<string> scenes;

    private void Awake()
    {
        foreach (string scene in scenes)
        {
            if (!SceneManager.GetSceneByName(scene).IsValid())
            {
                SceneManager.LoadScene(scene, LoadSceneMode.Additive);
            }
        }
    }
}
