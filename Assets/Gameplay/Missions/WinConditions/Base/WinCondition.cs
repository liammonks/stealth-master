using UnityEngine;

public abstract class WinCondition : ScriptableObject {

    public delegate void OnComplete();
    public event OnComplete onComplete;

    protected bool m_IsComplete = false;
    public bool IsComplete => m_IsComplete;

    protected virtual void OnEnable()
    {
        m_IsComplete = false;
    }

    protected void Complete()
    {
        if (m_IsComplete) return;
        m_IsComplete = true;
        onComplete.Invoke();
    }
}