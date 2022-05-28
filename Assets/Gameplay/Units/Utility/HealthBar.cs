using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image currentHealthImage, lerpedHealthImage;
    private const float lerpDuration = 5.0f;
    private Coroutine lerpCoroutine;
    
    public void UpdateHealth(float currentHealth, float maxHealth) {
        if (!gameObject.activeInHierarchy) { return; }
        float percentage = currentHealth / maxHealth;
        currentHealthImage.fillAmount = percentage;
        if (lerpCoroutine != null) { StopCoroutine(lerpCoroutine); }
        lerpCoroutine = StartCoroutine(LerpHealth(percentage));
    }
    
    private IEnumerator LerpHealth(float percentage) {
        float t = 0.0f;
        while(t < 1.0f) {
            t = Mathf.Min(t + (Time.deltaTime / lerpDuration), 1.0f);
            lerpedHealthImage.fillAmount = Mathf.Lerp(lerpedHealthImage.fillAmount, percentage, t);
            yield return null;
        }
    }
}
