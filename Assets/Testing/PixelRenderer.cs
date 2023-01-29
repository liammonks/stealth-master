using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PixelRenderer : MonoBehaviour
{
    [SerializeField]
    private Vector2Int m_PixelCount = new Vector2Int(64, 64);

    [SerializeField]
    private float m_CameraSize = 1.0f;

    [SerializeField]
    private Vector2 m_CameraOffset = new Vector2(0, 1);

    private Camera m_Camera;
    private RenderTexture m_RenderTexture;
    private RenderTexture m_OutputTexture;
    private Canvas m_Canvas;
    private RawImage m_Image;

    private void Awake()
    {
        SetLayer(transform);

        CreateCamera();
        CreateCanvas();
        CreateImage();

        m_OutputTexture = new RenderTexture(64, 64, 0, RenderTextureFormat.Default);
        m_OutputTexture.filterMode = FilterMode.Point;
        m_OutputTexture.depth = 16;
        m_OutputTexture.enableRandomWrite = true;
    }

    private void SetLayer(Transform root)
    {
        root.gameObject.layer = LayerMask.NameToLayer("Pixel");
        foreach (Transform child in root)
        {
            SetLayer(child);
        }
    }

    private void CreateCamera()
    {
        m_Camera = new GameObject("Pixel Camera", typeof(Camera)).GetComponent<Camera>();
        m_Camera.transform.SetParent(transform);
        m_Camera.transform.localPosition = new Vector3(m_CameraOffset.x, m_CameraOffset.y, -10);
        m_Camera.orthographic = true;
        m_Camera.orthographicSize = m_CameraSize;
        m_Camera.clearFlags = CameraClearFlags.SolidColor;
        m_Camera.backgroundColor = Color.clear;

        // Assign new render texture
        m_RenderTexture = new RenderTexture(m_PixelCount.x, m_PixelCount.y, 0, RenderTextureFormat.Default);
        m_RenderTexture.filterMode = FilterMode.Point;
        m_RenderTexture.depth = 16;
        m_RenderTexture.enableRandomWrite = true;
        m_Camera.targetTexture = m_RenderTexture;
    }

    private void CreateCanvas()
    {
        m_Canvas = new GameObject("Pixel Canvas", typeof(Canvas)).GetComponent<Canvas>();
        m_Canvas.transform.SetParent(transform);
        m_Canvas.transform.localPosition = new Vector3(2 + m_CameraOffset.x, m_CameraOffset.y, 0);
        m_Canvas.pixelPerfect = true;
        m_Canvas.renderMode = RenderMode.WorldSpace;
        m_Canvas.worldCamera = Camera.main;
        m_Canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(m_CameraSize * 2 * m_Camera.aspect, m_CameraSize * 2);
    }

    private void CreateImage()
    {
        m_Image = new GameObject("Image", typeof(RawImage)).GetComponent<RawImage>();
        m_Image.transform.SetParent(m_Canvas.transform);
        m_Image.transform.localPosition = Vector3.zero;

        m_Image.texture = m_RenderTexture;
        m_Image.rectTransform.anchorMin = Vector2.zero;
        m_Image.rectTransform.anchorMax = Vector2.one;
        m_Image.rectTransform.offsetMin = Vector2.zero;
        m_Image.rectTransform.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        
    }
}
