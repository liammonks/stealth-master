#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[ExecuteInEditMode]
public class LevelManagerEditor : MonoBehaviour
{

    private void Start()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Player.unity", OpenSceneMode.Additive);
            EditorSceneManager.OpenScene("Assets/Engine/Debugger/Debugger.unity", OpenSceneMode.Additive);
        }
    }

}

#endif
