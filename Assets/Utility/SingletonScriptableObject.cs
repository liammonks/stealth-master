using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = Resources.Load(typeof(T).Name) as T;
            }
            return m_Instance;
        }
    }

    private static T m_Instance;
}
