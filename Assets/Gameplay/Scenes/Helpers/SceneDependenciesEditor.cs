using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode] [RequireComponent(typeof(SceneDependencies))]
public class SceneDependenciesEditor : MonoBehaviour
{    
    private void Update() {
        #if UNITY_EDITOR
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
