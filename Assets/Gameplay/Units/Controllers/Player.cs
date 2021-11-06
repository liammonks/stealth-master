using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private Unit playerUnit;

    private InputData unitInputData;
    private Coroutine enableCrawlCoroutine;

    private void Awake()
    {
        unitInputData = playerUnit.GetInputData();
    }

    private void OnMovement(InputValue value)
    {
        unitInputData.movement = Mathf.CeilToInt(value.Get<Vector2>().x);
    }

    private void OnRun(InputValue value)
    {
        unitInputData.running = value.Get<float>() == 1.0f;
    }

    private void OnJump(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            unitInputData.jumpRequestTime = Time.unscaledTime;
        }
    }

    private void OnCrawl(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            // Set crawling
            if (Time.unscaledTime - unitInputData.crawlRequestTime > 0.6f)
            {
                unitInputData.crawling = true;
                unitInputData.crawlRequestTime = Time.unscaledTime;
            }
            else
            {
                // We tried crawling too quickly after the last crawl input
                enableCrawlCoroutine = StartCoroutine(EnableCrawlDelay(Time.unscaledTime - unitInputData.crawlRequestTime));
            }
        }
        else
        {
            unitInputData.crawling = false;
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
        unitInputData.crawling = true;
        unitInputData.crawlRequestTime = Time.unscaledTime;
    }

    private void OnGadgetPrimary(InputValue value) 
    {
        if (value.Get<float>() == 1.0f)
        {
            playerUnit.GadgetPrimary();
        }
    }

    private void OnGadgetSecondary(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            playerUnit.GadgetSecondary();
        }
    }
    
    private void OnMelee(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            unitInputData.meleeRequestTime = Time.unscaledTime;
        }
    }

    private void OnMouseMove(InputValue value)
    {

    }

    private void OnInteract(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            playerUnit.Interact();
        }
    }
    
}
