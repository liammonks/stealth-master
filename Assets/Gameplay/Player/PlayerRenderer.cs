using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private float scalar = 1.0f;
    
    private void Awake()
    {
        renderTexture.width = Mathf.RoundToInt(Screen.width * scalar);
        renderTexture.height = Mathf.RoundToInt(Screen.height * scalar);
    }

}
