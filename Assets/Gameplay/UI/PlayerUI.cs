using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance;

    private void Awake() {
        if (Instance != null)
        {
            Debug.LogError("Two Instances of GameplayUI Found");
            return;
        }
        Instance = this;
    }

    #region Menu

    [SerializeField] private GameObject menuRoot;

    public void OnMenu()
    {
        menuRoot.SetActive(!menuRoot.activeInHierarchy);
    }
    
    public void OnExit()
    {
        SceneManager.LoadScene("Planning", LoadSceneMode.Single);
    }
    
    #endregion

    #region Interaction

    [SerializeField] private Image interactPopup;
    private Vector2 interactPopupOffset = new Vector2(0.5f, 0.5f);
    private Transform interactPopupTarget;
    
    public void EnableInteractPopup(Transform target) {
        interactPopupTarget = target;
        interactPopup.gameObject.SetActive(true);
    }
    
    public void DisableInteractPopup() {
        interactPopupTarget = null;
        interactPopup.gameObject.SetActive(false);
    }

    #endregion
    
    private void Update() {
        if(interactPopupTarget != null) {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(interactPopupTarget.position + (Vector3)interactPopupOffset);
            interactPopup.rectTransform.position = screenPosition;
        }
    }
}
