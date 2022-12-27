using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkTimeDebug : MonoBehaviour
{
    [SerializeField]
    private Image left, right, top, bottom;

    private void Update()
    {
        float timeDecimal = Simulation.Time - (float)Math.Truncate(Simulation.Time);
        left.color = (timeDecimal >= 0.0f && timeDecimal < 0.25f) ? Color.white : Color.black;
        top.color = (timeDecimal >= 0.25f && timeDecimal < 0.5f) ? Color.white : Color.black;
        right.color = (timeDecimal >= 0.5f && timeDecimal < 0.75f) ? Color.white : Color.black;
        bottom.color = (timeDecimal >= 0.75f && timeDecimal < 1.0f) ? Color.white : Color.black;
    }
}
