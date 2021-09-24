using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[ExecuteAlways]
public class LevelManager : MonoBehaviour
{
    private void Awake()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Player.unity", OpenSceneMode.Additive);
        }
        #endif

        #if !UNITY_EDITOR
        if (!SceneManager.GetSceneByName("Player").IsValid())
        {
            SceneManager.LoadScene("Player", LoadSceneMode.Additive);
        }
        #endif
    }
}
