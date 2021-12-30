using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 3.0f;
    
    private void Awake() {
        StartCoroutine(DestroyCoroutine());
    }
    
    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
