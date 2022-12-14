using Debugging;
using System;

using UnityEngine;
using UnityEngine.InputSystem;

public class DebugToggle : MonoBehaviour
{
    public bool Active => m_CanvasObject.activeInHierarchy;
    public Action OnActivated;
    public Action OnDeactivated;

    [SerializeField]
    private GameObject m_CanvasObject;

    private void Awake()
    {
        Deactivate();
    }

    public void Activate()
    {
        DebugGUI.Hide();
        m_CanvasObject.SetActive(true);
        OnActivated?.Invoke();
    }

    public void Deactivate()
    {
        DebugGUI.Show();
        m_CanvasObject.SetActive(false);
        OnDeactivated?.Invoke();
    }

    private void OnToggleInput(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            if (m_CanvasObject.activeSelf)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }
    }
}
