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

    public float Movement => m_Movement;
    public bool Running => m_Running;
    public Vector2 MouseWorldPosition => m_MouseWorldPosition;
    public bool Melee => m_MeleeRequestTime >= (Time.unscaledTime - ActivationDuration);
    public bool GadgetPrimary => m_GadgetPrimary;
    public bool GadgetSecondary => m_GadgetSecondary;
    public bool Crawling => m_Crawling;
    public bool Jumping
    {
        get
        {
            return m_JumpRequestTime >= (Time.unscaledTime - ActivationDuration);
        }
        set
        {
            if (value == false) { m_JumpRequestTime = -1.0f; }
        }
    }

    private float m_Movement;
    private bool m_Running;
    private bool m_Crawling;
    private float m_JumpRequestTime = -1.0f;
    private float m_MeleeRequestTime = -1.0f;
    private Vector3 m_MouseScreenPosition;
    private Vector3 m_MouseWorldPosition;
    private bool m_GadgetPrimary;
    private bool m_GadgetSecondary;

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
            PlayerInput playerInput = gameObject.AddComponent<PlayerInput>();
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
        m_Movement = value.Get<float>();

        float abs = Mathf.Abs(m_Movement);
        if (abs > 1.0f)
        {
            m_Movement = Mathf.Clamp(m_Movement, -1.0f, 1.0f);
            m_Running = abs > 4.0f;
        }
    }

    private void OnRun(InputValue value)
    {
        m_Running = value.Get<float>() == 1.0f;
    }

    private void OnJump(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            m_JumpRequestTime = Time.unscaledTime;
        }
    }

    private void OnCrawl(InputValue value)
    {
        m_Crawling = value.Get<float>() == 1.0f;
    }

    private void OnMelee(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            m_MeleeRequestTime = Time.unscaledTime;
        }
    }

    private void OnMouseMove(InputValue value)
    {
        m_MouseScreenPosition = value.Get<Vector2>();
        m_MouseScreenPosition.z = 1.0f;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(m_MouseScreenPosition);
        Vector3 direction = (worldPos - Camera.main.transform.position).normalized;

        Ray ray = Camera.main.ScreenPointToRay(m_MouseScreenPosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, 0.25f));
        if (plane.Raycast(ray, out float distance))
        {
            m_MouseWorldPosition = Camera.main.transform.position + (direction * distance);
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
        m_GadgetPrimary = value.Get<float>() == 1.0f;
    }

    private void OnGadgetSecondary(InputValue value)
    {
        m_GadgetSecondary = value.Get<float>() == 1.0f;
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