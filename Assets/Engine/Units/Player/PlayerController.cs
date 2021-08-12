using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer renderer;
    
    [Header("Stats")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    
    [Header("Physics")]
    [SerializeField] private Vector2 velocity;
    [SerializeField] private float friction;
    [SerializeField] private float jumpForce = 2.0f;
    
    private Vector2 size = new Vector2(1, 1);
    private Vector2 moveInput;
    private bool grounded = true;
    private bool running = false;

    private void Update()
    {
        if(grounded)
        {
            if(velocity.x <= friction)
            {
                velocity.x = 0;
            }
            Vector2 movement = new Vector2(moveInput.x, 0);
            movement *= running ? runSpeed : walkSpeed;
            velocity = movement;
        }
        // Move
        transform.Translate(velocity * Time.deltaTime);
        // Animation
        animator.SetFloat("SpeedX", Mathf.Abs(velocity.x));
        animator.SetFloat("SpeedY", Mathf.Abs(velocity.y));
    }
    
    private void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        
        if(moveInput == Vector2.zero)
        {
            animator.Play("StealthMaster_Idle");
        }
        if(moveInput == Vector2.right)
        {
            renderer.flipX = false;
            animator.Play("StealthMaster_Run");
        }
        if(moveInput == Vector2.left)
        {
            renderer.flipX = true;
            animator.Play("StealthMaster_Run");
        }
    }
    
    private void OnRun(InputValue value)
    {
        running = value.Get<float>() == 1.0f;
    }
    
    private void OnJump(InputValue value)
    {
        if(value.Get<float>() == 1.0f)
        {
            velocity.y += jumpForce;
            grounded = false;
        }
    }
}
