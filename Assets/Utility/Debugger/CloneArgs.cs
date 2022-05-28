// This should be editor only
#if UNITY_EDITOR
using UnityEngine;
using ParrelSync;

[ExecuteAlways]
public class CloneArgs : MonoBehaviour
{

    void Start()
    {
        if (!ClonesManager.IsClone()) { return; }
        
        string arg = ClonesManager.GetArgument();
        Debug.Log("Clone Args: " + arg);
    }
    
}
#endif