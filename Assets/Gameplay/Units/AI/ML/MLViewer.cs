using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MLViewer : MonoBehaviour
{
    [SerializeField] private Camera target;
    
    private void Start() {
        GetComponent<RawImage>().texture = target.targetTexture;
    }
}
