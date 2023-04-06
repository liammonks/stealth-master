using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTimeDebugBehaviour : SingletonBehaviour<NetworkTimeDebugBehaviour>
{
    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
