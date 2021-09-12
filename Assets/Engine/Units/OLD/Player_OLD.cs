using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_OLD : Unit_OLD
{
    private Vector2 interactDirection;
    private Vector3 mouseWorldPosition;

    protected override void Update()
    {
        base.Update();
    }

    #region Venting
    private Vent currentVent = null;

    public void EnterVent(Vent enteredVent)
    {
        currentVent = enteredVent;
        transform.position = new Vector3(currentVent.transform.position.x, currentVent.transform.position.y, transform.position.z);
        SetVisible(false);
        EnableMovement(false);
    }

    public void ExitVent()
    {
        currentVent = null;
        SetVisible(true);
        EnableMovement(true);
    }
    
    public Vent GetCurrentVent()
    {
        return currentVent;
    }
    
    #endregion

    #region Input

    private void OnMovement(InputValue value)
    {
        if (currentVent == null)
        {
            // Normal Movement
            SetMoveDirection(Mathf.RoundToInt(value.Get<Vector2>().x));
        }
        else
        {
            // Vent Movement
            Vent nextVent = currentVent.GetConnectedVent(value.Get<Vector2>());
            if (nextVent != null)
            {
                EnterVent(nextVent);
            }
        }
    }
    
    private void OnRun(InputValue value)
    {
        SetRunning(value.Get<float>() == 1.0f);
    }
    
    private void OnJump(InputValue value)
    {
        if(value.Get<float>() == 1.0f)
        {
            Jump();
        }
    }
    
    private void OnCrawl(InputValue value)
    {
        Crawl(value.Get<float>() == 1.0f);
    }
    
    private void OnMouseMove(InputValue value)
    {
        Vector2 mouseDirection = new Vector2
        {
          x = value.Get<Vector2>().x - (Screen.width * 0.5f),
          y = value.Get<Vector2>().y - (Screen.height * 0.5f)
        };
        interactDirection = mouseDirection.normalized;
    }
    
    private void OnInteract(InputValue value)
    {
        if(value.Get<float>() == 1.0f)
        {
            RaycastHit2D interactionRayHit = Physics2D.Raycast(transform.position, interactDirection, interactionDistance, interactionMask);
            if (interactionRayHit)
            {
                // Interactable hit
                Debug.DrawRay(transform.position, interactDirection * interactionDistance, Color.green, 1.0f);
                interactionRayHit.collider.GetComponent<Interactable>().OnInteract(this);
            }
            else
            {
                Debug.DrawRay(transform.position, interactDirection * interactionDistance, Color.blue, 1.0f);
            }
        }
    }
    
    #endregion
}
