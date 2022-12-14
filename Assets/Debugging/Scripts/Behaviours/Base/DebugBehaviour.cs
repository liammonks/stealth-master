using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class DebugBehaviour<T> : MonoBehaviour
{
    public static DebugBehaviour<T> Instance => m_Instance;
    protected static DebugBehaviour<T> m_Instance;

    public void Enable()
    {
        enabled = true;
    }

    public void Disable()
    {
        enabled = false;
    }

    public void Toggle()
    {
        enabled = !enabled;
    }

    protected virtual void Awake()
    {
        enabled = false;
        if (m_Instance != null)
        {
            Debug.LogError($"Two instances of {GetType()} found!", this);
            return;
        }
        m_Instance = this;
    }
}
