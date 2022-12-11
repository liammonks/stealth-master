using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class OverlayCamera : MonoBehaviour
{
    [SerializeField]
    private RawImage m_OverlayImage;

    private RenderTexture m_RenderTexture;

    private void Awake()
    {
        m_RenderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        m_RenderTexture.antiAliasing = 8;
        m_RenderTexture.filterMode = FilterMode.Point;
        GetComponent<Camera>().targetTexture = m_RenderTexture;
        m_OverlayImage.texture = m_RenderTexture;
        m_OverlayImage.enabled = true;
    }

}
