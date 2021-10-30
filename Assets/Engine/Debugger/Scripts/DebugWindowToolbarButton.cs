using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWindowToolbarButton : MonoBehaviour
{
    [SerializeField] private DebugWindowType windowType;
    
    public void Initialise(DebugWindowType debugWindowType) {
        windowType = debugWindowType;
    }
    
    public void OpenWindow() {
        DebugWindowManager.Instance.OpenWindow(windowType);
    }
    
    public void CloseWindow() {
        DebugWindowManager.Instance.CloseWindow(windowType);
    }
}
