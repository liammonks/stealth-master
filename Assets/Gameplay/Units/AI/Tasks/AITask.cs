using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITask : MonoBehaviour
{
    public delegate void OnTaskComplete();
    public event OnTaskComplete onTaskComplete;

    public float interactionDistance = 0.5f;

    [SerializeField] private bool m_Cooldown = false;
    
    [SerializeField] private float executeCooldown = 60.0f;
    [SerializeField] private float executeDuration = 5.0f;

    public bool IsAvailable()
    {
        return !m_Cooldown;
    }

    public void Execute()
    {
        StartCoroutine(ExecuteCoroutine());
    }
    
    private IEnumerator ExecuteCoroutine()
    {
        m_Cooldown = true;
        yield return new WaitForSeconds(executeDuration);
        onTaskComplete.Invoke();
        yield return new WaitForSeconds(executeCooldown);
        m_Cooldown = false;
    }

}
