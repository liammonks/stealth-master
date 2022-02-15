using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITask : MonoBehaviour
{
    public bool Cooldown => m_Cooldown;
    private bool m_Cooldown = false;
    
    [SerializeField] private float executeCooldown = 60.0f;
    [SerializeField] private float executeDuration = 5.0f;

    public float Execute()
    {
        StartCoroutine(CooldownCoroutine());
        return executeDuration;
    }
    
    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(executeCooldown);
        m_Cooldown = false;
    }
}
