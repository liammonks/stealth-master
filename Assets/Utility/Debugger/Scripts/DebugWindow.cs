using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DebugWindow : MonoBehaviour, IPointerDownHandler
{
    private const int edgeThickness = 4;

    [SerializeField] private TextMeshProUGUI logText, titleText;
    [SerializeField] private GameObject leftEdgeImage, rightEdgeImage, bottomEdgeImage, maxButton, minButton;
    
    private int maxLineCount = 5;
    private int currentLine = 0;
    private string[] lines;
    private Vector2 defaultOffsetMin, defaultOffsetMax;

    public void Initialise(string title) {
        Vector2 leftEdgeSize = leftEdgeImage.GetComponent<Image>().rectTransform.sizeDelta;
        leftEdgeSize.x = edgeThickness;
        leftEdgeImage.GetComponent<Image>().rectTransform.sizeDelta = leftEdgeSize;
        Vector2 rightEdgeSize = rightEdgeImage.GetComponent<Image>().rectTransform.sizeDelta;
        rightEdgeSize.x = edgeThickness;
        rightEdgeImage.GetComponent<Image>().rectTransform.sizeDelta = rightEdgeSize;
        Vector2 bottomEdgeSize = bottomEdgeImage.GetComponent<Image>().rectTransform.sizeDelta;
        bottomEdgeSize.y = edgeThickness;
        bottomEdgeImage.GetComponent<Image>().rectTransform.sizeDelta = bottomEdgeSize;

        titleText.text = title;
        UpdateLineCount();
    }

    public void SetText(string text) {
        lines[currentLine] = text;
        UpdateText();
    }
    
    private void UpdateText() {
        logText.text = string.Empty;
        foreach (string line in lines)
        {
            logText.text += line + '\n';
        }
    }
    
    private void UpdateLineCount() {
        currentLine = 0;
        maxLineCount = Mathf.Max(1, Mathf.RoundToInt(logText.GetComponent<RectTransform>().rect.height / logText.fontSize));
        lines = new string[maxLineCount];
        UpdateText();
    }
    
    public string GetCurrentText() {
        if (lines[currentLine] == null) {
            return string.Empty;
        }
        return lines[currentLine];
    }
    
    public void Append() {
        currentLine++;
        if(currentLine == maxLineCount) {
            for (int i = 1; i < maxLineCount; ++i)
            {
                lines[i - 1] = lines[i];
            }
            currentLine--;
        }        
    }
    
    public void Maximise() {
        // Store previous offsets
        defaultOffsetMin = GetComponent<RectTransform>().offsetMin;
        defaultOffsetMax = GetComponent<RectTransform>().offsetMax;
        
        GetComponent<RectTransform>().offsetMin = Vector2.zero;
        GetComponent<RectTransform>().offsetMax = new Vector2(0, -DebugWindowManager.toolbarThickness);
        UpdateLineCount();
        minButton.SetActive(true);
        maxButton.SetActive(false);
    }
    
    public void Minimise() {
        GetComponent<RectTransform>().offsetMin = defaultOffsetMin;
        GetComponent<RectTransform>().offsetMax = defaultOffsetMax;
        UpdateLineCount();
        minButton.SetActive(false);
        maxButton.SetActive(true);
    }

    public void Close() {
        gameObject.SetActive(false);
    }
    
    public void SetMinOffset(Vector2 minOffset) {
        GetComponent<RectTransform>().offsetMin = minOffset;
        UpdateLineCount();
    }
    public void SetMaxOffset(Vector2 maxOffset) {
        GetComponent<RectTransform>().offsetMax = maxOffset;
        UpdateLineCount();
    }
    public Vector2 GetMinOffset() {
        return GetComponent<RectTransform>().offsetMin;
    }
    public Vector2 GetMaxOffset() {
        return GetComponent<RectTransform>().offsetMax;
    }

    #region Dragging

    private Vector2 currentMousePosition;
    private Vector2 dragOffset;
    private bool mouseDown, dragging;
    private bool leftEdge, rightEdge, bottomEdge;
    private Vector2 lastMousePosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!leftEdge && !rightEdge && !bottomEdge)
        {
            dragOffset = (Vector2)GetComponent<RectTransform>().position - currentMousePosition;
            dragging = true;
        }
    }

    public void OnMouseDown(InputValue value) {
        mouseDown = value.Get<float>() == 1.0f;
        if(!mouseDown)
        {
            dragging = false;
        }
    }
    
    private void OnMouseMove(InputValue value) {
        currentMousePosition = value.Get<Vector2>();
        Vector2 mouseDelta = currentMousePosition - lastMousePosition;

        if (mouseDown)
        {
            if (dragging)
            {
                GetComponent<RectTransform>().position = currentMousePosition + dragOffset;
            }
            else
            {
                if (leftEdge || rightEdge || bottomEdge)
                {
                    if (leftEdge)
                    {
                        GetComponent<RectTransform>().offsetMin = new Vector2(GetComponent<RectTransform>().offsetMin.x + mouseDelta.x, GetComponent<RectTransform>().offsetMin.y);
                    }
                    if (rightEdge)
                    {
                        GetComponent<RectTransform>().offsetMax = new Vector2(GetComponent<RectTransform>().offsetMax.x + mouseDelta.x, GetComponent<RectTransform>().offsetMax.y);
                    }
                    if (bottomEdge)
                    {
                        GetComponent<RectTransform>().offsetMin = new Vector2(GetComponent<RectTransform>().offsetMin.x, GetComponent<RectTransform>().offsetMin.y + mouseDelta.y);
                    }
                    UpdateLineCount();
                }
            }
        }
        else
        {
            // Are we hovering an edge?
            // Left
            float dist = Mathf.Abs(currentMousePosition.x - (GetComponent<RectTransform>().position.x - (GetComponent<RectTransform>().rect.width * 0.5f)));
            if (dist <= edgeThickness)
            {
                leftEdge = true;
                leftEdgeImage.gameObject.SetActive(true);
            }
            else
            {
                leftEdge = false;
                leftEdgeImage.gameObject.SetActive(false);
            }
            // Right
            dist = Mathf.Abs(currentMousePosition.x - (GetComponent<RectTransform>().position.x + (GetComponent<RectTransform>().rect.width * 0.5f)));
            if (dist <= edgeThickness)
            {
                rightEdge = true;
                rightEdgeImage.gameObject.SetActive(true);
            }
            else
            {
                rightEdge = false;
                rightEdgeImage.gameObject.SetActive(false);
            }
            // Bottom
            dist = Mathf.Abs(currentMousePosition.y - (GetComponent<RectTransform>().position.y - (GetComponent<RectTransform>().rect.height * 0.5f)));
            if (dist <= edgeThickness)
            {
                bottomEdge = true;
                bottomEdgeImage.gameObject.SetActive(true);
            }
            else
            {
                bottomEdge = false;
                bottomEdgeImage.gameObject.SetActive(false);
            }
        }
        lastMousePosition = currentMousePosition;
    }
    
    #endregion
}
