using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugWindowToolbarButton : MonoBehaviour
{
    [SerializeField] private DebugWindowType windowType;
    [SerializeField] private TextMeshProUGUI title;

    public void Initialise(DebugWindowType debugWindowType) {
        windowType = debugWindowType;
        title.text = debugWindowType.ToString();
    }
    
    public void OpenWindow() {
        DebugWindowManager.Instance.OpenWindow(windowType);
    }
    
    public void CloseWindow() {
        DebugWindowManager.Instance.CloseWindow(windowType);
    }
}
