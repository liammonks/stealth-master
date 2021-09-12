using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Unit))]
public class Player : MonoBehaviour
{
    private InputData unitInputData;

    private void Awake()
    {
        unitInputData = GetComponent<Unit>().GetInputData();
    }

    private void OnMovement(InputValue value)
    {
        unitInputData.movement = Mathf.CeilToInt(value.Get<Vector2>().x);
        
        // Jumping
        if(value.Get<Vector2>().y > 0)
        {
            unitInputData.jumpQueued = true;
        }

        // Crawling
        if (value.Get<Vector2>().y < 0)
        {
            unitInputData.crawlQueued = true;
        }
    }

    private void OnRun(InputValue value)
    {
        unitInputData.running = value.Get<float>() == 1.0f;
    }

    private void OnJump(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            unitInputData.jumpQueued = true;
        }
    }

    private void OnCrawl(InputValue value)
    {

    }

    private void OnMouseMove(InputValue value)
    {

    }

    private void OnInteract(InputValue value)
    {

    }
    
}
