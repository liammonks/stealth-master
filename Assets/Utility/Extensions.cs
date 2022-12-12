using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Extensions
{
    #region GameObjects

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject.TryGetComponent<T>(out T t))
        {
            return t;
        }
        else
        {
            return gameObject.AddComponent<T>();
        }
    }

    #endregion

}