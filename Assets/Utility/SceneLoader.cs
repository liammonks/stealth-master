using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneLoader
{
    [MenuItem("Stealth Master/Load Client")]
    static void LoadClientScenes()
    {
        EditorSceneManager.OpenScene("Assets/Networking/Scenes/Client.unity", OpenSceneMode.Additive);
    }

    [MenuItem("Stealth Master/Load Server")]
    static void LoadServerScenes()
    {
        EditorSceneManager.OpenScene("Assets/Networking/Scenes/Server.unity", OpenSceneMode.Additive);
    }
}
