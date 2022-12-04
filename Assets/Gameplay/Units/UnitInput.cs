using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UnitInput : MonoBehaviour
{
    public class UnitInputData
    {
        public float Movement = 0;
        public bool Running = false;
        public bool Crawling = false;
        public bool Jumping = false;
        public bool Melee = false;
        public Vector3 MouseWorldPosition = Vector3.zero;
        public bool GadgetPrimary = false;
        public bool GadgetSecondary = false;

        public void SetData(UnitInputData data)
        {
            Movement = data.Movement;
            Running = data.Running;
            Crawling = data.Crawling;
            Jumping = data.Jumping;
            Melee = data.Melee;
            MouseWorldPosition = data.MouseWorldPosition;
            GadgetPrimary = data.GadgetPrimary;
            GadgetSecondary = data.GadgetSecondary;
        }
    }

    public static readonly float ActivationDuration = 0.2f;
    public static readonly float KyoteTime = 0.2f;

    public bool PlayerControlled => m_PlayerControlled;
    [SerializeField] private bool m_PlayerControlled;

    public float Movement => m_CurrentData.Movement;
    public bool Running => m_CurrentData.Running;
    public bool Crawling => m_CurrentData.Crawling;
    public bool Jumping => m_CurrentData.Jumping;
    public bool Melee => m_CurrentData.Melee;
    public Vector2 MouseWorldPosition => m_CurrentData.MouseWorldPosition;
    public bool GadgetPrimary => m_CurrentData.GadgetPrimary;
    public bool GadgetSecondary => m_CurrentData.GadgetSecondary;

    private UnitInputData m_CurrentData = new UnitInputData();

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
        m_CurrentData.Movement = Mathf.Clamp(value.Get<float>(), -1.0f, 1.0f);
    }

    private void OnRun(InputValue value)
    {
        m_CurrentData.Running = value.Get<float>() == 1.0f;
    }

    private void OnJump(InputValue value)
    {
        m_CurrentData.Jumping = value.Get<float>() == 1.0f;
    }

    private void OnCrawl(InputValue value)
    {
        m_CurrentData.Crawling = value.Get<float>() == 1.0f;
    }

    private void OnMelee(InputValue value)
    {
        m_CurrentData.Melee = value.Get<float>() == 1.0f;
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
            m_CurrentData.MouseWorldPosition = Camera.main.transform.position + (direction * distance);
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
        m_CurrentData.GadgetPrimary = value.Get<float>() == 1.0f;
    }

    private void OnGadgetSecondary(InputValue value)
    {
        m_CurrentData.GadgetSecondary = value.Get<float>() == 1.0f;
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

    #region External Control

    public void ResetJumping()
    {
        m_CurrentData.Jumping = false;
    }

    #endregion
}