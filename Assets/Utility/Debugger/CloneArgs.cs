// This should be editor only
#if UNITY_EDITOR
using UnityEngine;
using ParrelSync;

[ExecuteAlways]
public class CloneArgs : MonoBehaviour
{

    void Start()
    {
        if (!ParrelSync.ClonesManager.IsClone()) { return; }
        
        string arg = ClonesManager.GetArgument();
        Debug.Log("Clone Args: " + arg);
        switch(arg)
        {
            case "client":
                NetworkManager.networkType = NetworkType.Client;
                break;
            case "host":
                NetworkManager.networkType = NetworkType.Host;
                break;
            case "server":
                NetworkManager.networkType = NetworkType.Server;
                break;
        }
    }
    
}
#endif