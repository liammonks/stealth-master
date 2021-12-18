using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Unit
{
    public static Vector3 MousePosition;

    [Header("Player")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private int equippedGadgetIndex = -1;
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    private Coroutine enableCrawlCoroutine;

    protected override void Start()
    {
        base.Start();
        data.hitMask = LayerMask.GetMask("Enemy");
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        Log.UnitState(state, data.stateDuration);
    }

    public override void Die()
    {
        LevelManager.Instance.RespawnPlayer();
    }

    private void OnMovement(InputValue value)
    {
        data.input.movement = Mathf.CeilToInt(value.Get<Vector2>().x);
    }

    private void OnRun(InputValue value)
    {
        data.input.running = value.Get<float>() == 1.0f;
    }

    private void OnJump(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            data.input.jumpRequestTime = Time.unscaledTime;
        }
    }

    private void OnCrawl(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            // Set crawling
            if (Time.unscaledTime - data.input.crawlRequestTime > 0.6f)
            {
                data.input.crawling = true;
                data.input.crawlRequestTime = Time.unscaledTime;
            }
            else
            {
                // We tried crawling too quickly after the last crawl input
                enableCrawlCoroutine = StartCoroutine(EnableCrawlDelay(Time.unscaledTime - data.input.crawlRequestTime));
            }
        }
        else
        {
            data.input.crawling = false;
            if(enableCrawlCoroutine != null)
            {
                StopCoroutine(enableCrawlCoroutine);
                enableCrawlCoroutine = null;
            }
        }
    }
    
    private IEnumerator EnableCrawlDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        data.input.crawling = true;
        data.input.crawlRequestTime = Time.unscaledTime;
    }

    private void OnGadgetPrimary(InputValue value) 
    {
        GadgetPrimary(value.Get<float>() == 1.0f);
    }

    private void OnGadgetSecondary(InputValue value)
    {
        GadgetSecondary(value.Get<float>() == 1.0f);
    }
    
    private void OnMelee(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            data.input.meleeRequestTime = Time.unscaledTime;
        }
    }

    private void OnMouseMove(InputValue value)
    {
        MousePosition = value.Get<Vector2>();
        MousePosition.z = -mainCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z;
    }

    private void OnInteract(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            Interact();
        }
    }
    
    public void SetCameraOffset(Vector2 offset)
    {
        cameraTarget.localPosition = offset;
    }
    
    public void OnNextGadget(InputValue value)
    {
        if(value.Get<float>() > 0)
        {
            int originalGadgetIndex = equippedGadgetIndex;
            bool newGadgetEquipped = false;
            equippedGadgetIndex++;
            
            if (equippedGadgetIndex == GlobalData.playerGadgets.Count)
            { 
                equippedGadgetIndex = -1;
                newGadgetEquipped = EquipGadget(GlobalData.DefaultGadget);
            }
            else
            {
                newGadgetEquipped = EquipGadget(GlobalData.playerGadgets[equippedGadgetIndex]);
            }

            if (!newGadgetEquipped) equippedGadgetIndex = originalGadgetIndex;
        }
    }

    public void OnPreviousGadget(InputValue value)
    {
        if (value.Get<float>() < 0)
        {
            int originalGadgetIndex = equippedGadgetIndex;
            bool newGadgetEquipped = false;
            equippedGadgetIndex--;
            
            switch (equippedGadgetIndex)
            {
                case -2:
                    equippedGadgetIndex = GlobalData.playerGadgets.Count - 1;
                    newGadgetEquipped = EquipGadget(GlobalData.playerGadgets[equippedGadgetIndex]);
                    break;
                case -1:
                    newGadgetEquipped = EquipGadget(GlobalData.DefaultGadget);
                    break;
                default:
                    newGadgetEquipped = EquipGadget(GlobalData.playerGadgets[equippedGadgetIndex]);
                    break;
            }
            
            if (!newGadgetEquipped) equippedGadgetIndex = originalGadgetIndex;
        }
    }
}
