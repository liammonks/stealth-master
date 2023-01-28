using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class NetworkSceneLoader
{

    #region Client

    private const string ClientActiveSetting = "Stealth Master/Client Active";

    public static bool ClientActive
    {
        get { return EditorPrefs.GetBool(ClientActiveSetting, true); }
        set { EditorPrefs.SetBool(ClientActiveSetting, value); }
    }

    [MenuItem(ClientActiveSetting)]
    private static void ToggleClientActive()
    {
        ClientActive = !ClientActive;
        if (!ClientActive)
        {
            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath("Assets/Networking/Scenes/Client.unity"), true);
        }

        if (ClientActive && ServerActive)
        {
            ToggleServerActive();
        }
    }

    [MenuItem(ClientActiveSetting, true)]
    private static bool ToggleClientActiveValidate()
    {
        Menu.SetChecked(ClientActiveSetting, ClientActive);
        return true;
    }

    #endregion

    #region Server

    private const string ServerActiveSetting = "Stealth Master/Server Active";

    public static bool ServerActive
    {
        get { return EditorPrefs.GetBool(ServerActiveSetting, true); }
        set { EditorPrefs.SetBool(ServerActiveSetting, value); }
    }

    [MenuItem(ServerActiveSetting)]
    private static void ToggleServerActive()
    {
        ServerActive = !ServerActive;

        if (!ServerActive)
        {
            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath("Assets/Networking/Scenes/Server.unity"), true);
        }
        
        if (ServerActive && ClientActive)
        {
            ToggleClientActive();
        }
    }

    [MenuItem(ServerActiveSetting, true)]
    private static bool ToggleServerActiveValidate()
    {
        Menu.SetChecked(ServerActiveSetting, ServerActive);
        return true;
    }

    #endregion

}
