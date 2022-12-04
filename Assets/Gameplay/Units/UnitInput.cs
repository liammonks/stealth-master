using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UnitInput : MonoBehaviour
{

    public static readonly float ActivationDuration = 0.2f;
    public static readonly float KyoteTime = 0.2f;

    public bool PlayerControlled => m_PlayerControlled;
    [SerializeField] private bool m_PlayerControlled;

    #region Properties
    public float Movement;
    public bool Running;
    public bool Crawling;
    public bool Jumping;
    public bool Melee;
    public Vector2 MouseWorldPosition;
    public bool GadgetPrimary;
    public bool GadgetSecondary;
    #endregion

    #region Events
    public Action<float> OnMovementChanged;
    public Action OnRunningChanged;
    public Action OnCrawlingChanged;
    public Action OnJumpingChanged;
    #endregion

    private void Start()
    {
        SetPlayerControl(m_PlayerControlled);
    }

    private void SetPlayerControl(bool playerControlled)
    {
        m_PlayerControlled = playerControlled;
        if (m_PlayerControlled)
        {
            StartCoroutine(LoadInput());
        }
        else
        {
            PlayerInput playerInput = gameObject.GetComponent<PlayerInput>();
            if (playerInput != null) { Destroy(playerInput); }
        }

        IEnumerator LoadInput()
        {
            PlayerInput playerInput = gameObject.GetOrAddComponent<PlayerInput>();
            AsyncOperationHandle<InputActionAsset> handle = Addressables.LoadAssetAsync<InputActionAsset>("InputActions");
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                playerInput.actions = handle.Result;
                playerInput.actions.Enable();
            }
        }
    }

    #region Input

    private void OnMovement(InputValue value)
    {
        Movement = Mathf.Clamp(value.Get<float>(), -1.0f, 1.0f);
        OnMovementChanged?.Invoke(Movement);
    }

    private void OnRun(InputValue value)
    {
        Running = value.Get<float>() == 1.0f;
    }

    private void OnJump(InputValue value)
    {
        Jumping = value.Get<float>() == 1.0f;
    }

    private void OnCrawl(InputValue value)
    {
        Crawling = value.Get<float>() == 1.0f;
    }

    private void OnMelee(InputValue value)
    {
        Melee = value.Get<float>() == 1.0f;
    }

    private void OnMouseMove(InputValue value)
    {
        Vector3 mouseScreenPosition = value.Get<Vector2>();
        mouseScreenPosition.z = 1.0f;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector3 direction = (worldPos - Camera.main.transform.position).normalized;

        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, 0.25f));
        if (plane.Raycast(ray, out float distance))
        {
            MouseWorldPosition = Camera.main.transform.position + (direction * distance);
        }
    }

    private void OnStickMove(InputValue value)
    {
        //Vector2 dir = value.Get<Vector2>();
        //if (dir.sqrMagnitude == 0) { return; }
        //m_MouseScreenPosition = (Vector2)Camera.main.WorldToScreenPoint(transform.position) + dir;
    }

    private void OnInteract(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {

        }
    }

    private void OnGadgetPrimary(InputValue value)
    {
        GadgetPrimary = value.Get<float>() == 1.0f;
    }

    private void OnGadgetSecondary(InputValue value)
    {
        GadgetSecondary = value.Get<float>() == 1.0f;
    }

    private void OnNextGadget(InputValue value)
    {

    }

    private void OnPreviousGadget(InputValue value)
    {

    }

    private void OnGadget_0()
    {

    }

    private void OnGadget_1()
    {

    }

    private void OnGadget_2()
    {

    }

    private void OnGadget_3()
    {

    }

    #endregion

}