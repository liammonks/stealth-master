using DarkRift;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UnitInput : MonoBehaviour, IRollback
{
    public struct UnitInputData : IDarkRiftSerializable
    {
        public float movement;
        public bool jumping;
        public bool running;
        public bool crawling;
        public bool melee;
        public Vector2 mouseWorldPosition;
        public bool gadgetPrimary;
        public bool gadgetSecondary;

        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.ReadSingle();
            e.Reader.ReadBoolean();
            e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(movement);
            e.Writer.Write(jumping);
            e.Writer.Write(running);
        }
    }

    public bool PlayerControlled => m_PlayerControlled;
    [SerializeField] private bool m_PlayerControlled;

    #region Properties
    [ShowInInspector]
    public float Movement => m_Data.movement;

    [ShowInInspector]
    public bool Jumping => m_Data.jumping;

    [ShowInInspector]
    public bool Running => m_Data.running;

    [ShowInInspector]
    public bool Crawling;

    public bool Melee;
    public Vector2 MouseWorldPosition;
    public bool GadgetPrimary;
    public bool GadgetSecondary;
    #endregion

    #region Fields
    private float m_MovementInput;
    private float m_MovementActivationTime = -1.0f;

    private bool m_JumpingInput;
    private float m_JumpingActivationTime = -1.0f;

    private bool m_RunningInput;
    private float m_RunningActivationTime = -1.0f;

    private bool m_Crawling;
    private bool m_Melee;
    private Vector2 m_MouseWorldPosition;
    private bool m_GadgetPrimary;
    private bool m_GadgetSecondary;
    #endregion

    private UnitInputData m_Data = new UnitInputData();

    public void Initialise()
    {
        Simulation.Instance.RegisterUnitInput(this);
        SetPlayerControl(m_PlayerControlled);
    }

    public void PrepareInput()
    {
       if (Simulation.Time - m_MovementActivationTime > Simulation.TimeStep) { m_Data.movement = m_MovementInput; }
       if (Simulation.Time - m_JumpingActivationTime > Simulation.TimeStep) { m_Data.jumping = false; }
       if (Simulation.Time - m_RunningActivationTime > Simulation.TimeStep) { m_Data.running = m_RunningInput; }
    }

    public List<StateData> GetSimulationState()
    {
        return new List<StateData> { new StateData(this, m_Data) };
    }

    public void SetSimulationState(IDarkRiftSerializable data)
    {
        m_Data = (UnitInputData)data;
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

    private void OnDestroy()
    {
        Simulation.Instance?.UnregisterUnitInput(this);
    }

    #region Input Listeners

    private void OnMovement(InputValue value)
    {
        m_MovementInput = Mathf.Clamp(value.Get<float>(), -1.0f, 1.0f);
        if (m_MovementInput != 0)
        {
            m_MovementActivationTime = Simulation.Time;
            m_Data.movement = m_MovementInput;
        }
    }

    private void OnJump(InputValue value)
    {
        m_JumpingInput = value.Get<float>() == 1.0f;
        if (m_JumpingInput)
        {
            m_JumpingActivationTime = Simulation.Time;
            m_Data.jumping = true;
        }
    }

    private void OnRun(InputValue value)
    {
        m_RunningInput = value.Get<float>() == 1.0f;
        if (m_RunningInput)
        {
            m_RunningActivationTime = Simulation.Time;
            m_Data.running = true;
        }
    }

    private void OnCrawl(InputValue value)
    {
        //Crawling = value.Get<float>() == 1.0f;
    }

    private void OnMelee(InputValue value)
    {
        //Melee = value.Get<float>() == 1.0f;
    }

    private void OnMouseMove(InputValue value)
    {
        //Vector3 mouseScreenPosition = value.Get<Vector2>();
        //mouseScreenPosition.z = 1.0f;

        //Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        //Vector3 direction = (worldPos - Camera.main.transform.position).normalized;

        //Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        //Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, 0.25f));
        //if (plane.Raycast(ray, out float distance))
        //{
        //    MouseWorldPosition = Camera.main.transform.position + (direction * distance);
        //}
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
        //GadgetPrimary = value.Get<float>() == 1.0f;
    }

    private void OnGadgetSecondary(InputValue value)
    {
        //GadgetSecondary = value.Get<float>() == 1.0f;
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