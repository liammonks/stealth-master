using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<T>(true);
            }
            return (T)m_Instance;
        }
    }

    private static SingletonBehaviour<T> m_Instance;

    protected virtual void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else
        {
            if (m_Instance != this)
            {
                Debug.LogError($"Two instances of {GetType()} found!");
            }
        }
    }

}
