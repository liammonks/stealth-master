using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Unit
{
    private Coroutine enableCrawlCoroutine;

    protected override void Awake()
    {
        base.Awake();
        data.hitMask = LayerMask.GetMask("Enemy");
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
        if (value.Get<float>() == 1.0f)
        {
            GadgetPrimary();
        }
    }

    private void OnGadgetSecondary(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            GadgetSecondary();
        }
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

    }

    private void OnInteract(InputValue value)
    {
        if (value.Get<float>() == 1.0f)
        {
            Interact();
        }
    }
    
}
