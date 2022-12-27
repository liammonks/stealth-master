using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicSettings : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 300;
        QualitySettings.vSyncCount = 0;
    }
}
