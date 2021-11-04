using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance;

    [SerializeField] private Image interactPopup;
    private Vector2 interactPopupOffset = new Vector2(0.5f, 0.5f);
    private Transform interactPopupTarget;

    private void Awake() {
        if (Instance != null)
        {
            Debug.LogError("Two Instances of GameplayUI Found");
            return;
        }
        Instance = this;
    }
    
    public void EnableInteractPopup(Transform target) {
        interactPopupTarget = target;
        interactPopup.gameObject.SetActive(true);
    }
    
    public void DisableInteractPopup() {
        interactPopupTarget = null;
        interactPopup.gameObject.SetActive(false);
    }
    
    private void Update() {
        if(interactPopupTarget != null) {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(interactPopupTarget.position + (Vector3)interactPopupOffset);
            interactPopup.rectTransform.position = screenPosition;
        }
    }
}
