using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer renderer;

    [Header("Stats")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float airAuthority = 2.0f;
    [SerializeField] private float climbHeight = 0.5f;

    [Header("Physics")]
    [SerializeField] private Vector2 size = new Vector2(0.5f, 1.7f);
    [SerializeField] private float airDrag = 1.0f;
    [SerializeField] private float jumpForce = 6.0f;

    private Vector2 velocity;
    private int moveDirection = 0;
    private bool grounded = false;
    private bool running = false;
    private bool canJump = false;
    private bool jumpFrame = false;
    private bool climbFrame = false;

    private void Update()
    {
        // Bounds
        Vector3 topLeft = transform.position + new Vector3(-size.x * 0.5f, size.y * 0.5f);
        Vector3 topRight = transform.position + new Vector3(size.x * 0.5f, size.y * 0.5f);
        Vector3 bottomRight = transform.position + new Vector3(size.x * 0.5f, -size.y * 0.5f);
        Vector3 bottomLeft = transform.position + new Vector3(-size.x * 0.5f, -size.y * 0.5f);

        Debug.DrawLine(topLeft, topRight);
        Debug.DrawLine(topRight, bottomRight);
        Debug.DrawLine(bottomRight, bottomLeft);
        Debug.DrawLine(bottomLeft, topLeft);

        if(climbFrame) // We climbed an object last frame, cancel out vertical momentum
        {
            velocity.y = 0;
            Debug.Log("CLIMB");
            climbFrame = false;
        }
        if (!jumpFrame) // Dont ground check if jumping
        {
            // Raycast Left
            float leftVelocity = 0.0f;
            RaycastHit2D leftHit = Physics2D.Raycast(topLeft, Vector2.down, size.y);
            if (leftHit)
            {
                Debug.DrawLine(topLeft, leftHit.point, Color.red);
                float incline = size.y - leftHit.distance;
                if (incline <= climbHeight)
                {
                    leftVelocity = incline / Time.deltaTime;
                }
                grounded = true;
                canJump = true;
            }
            // Raycast Right
            float rightVelocity = 0.0f;
            RaycastHit2D rightHit = Physics2D.Raycast(topRight, Vector2.down, size.y);
            if (rightHit)
            {
                Debug.DrawLine(topRight, rightHit.point, Color.red);
                float incline = size.y - rightHit.distance;
                if (incline <= climbHeight)
                {
                    rightVelocity = incline / Time.deltaTime;
                }
                grounded = true;
                canJump = true;
            }
            if (!leftHit && !rightHit)
            {
                grounded = false;
            }
            else
            {
                float climbVelocity = Mathf.Max(leftVelocity, rightVelocity);
                velocity.y = climbVelocity;
                if (climbVelocity > 0.1f)
                {
                    climbFrame = true;
                }
            }
        }
        else
        {
            // Reset jumpFrame
            jumpFrame = false;
        }

        if (grounded)
        {
            // Ground movement
            velocity.x = (running ? runSpeed : walkSpeed) * moveDirection;
        }
        else
        {
            // Air movement
            if (Mathf.Abs(velocity.x) < walkSpeed)
            {
                // Allow player to push towards walking speed while in the air
                velocity.x += (running ? runSpeed : walkSpeed) * moveDirection * Time.deltaTime * airAuthority;
            }
            else
            {
                // Apply drag
                velocity.x = Mathf.MoveTowards(velocity.x, 0.0f, Time.deltaTime * airDrag);
            }
            // Apply gravity
            velocity.y -= 9.81f * Time.deltaTime;
        }
        // Move
        velocity.y = Mathf.Clamp(velocity.y, -10.0f, 10.0f);
        transform.Translate(velocity * Time.deltaTime);
    }
    
    protected void Jump()
    {
        if (!canJump) { return; }
        canJump = false;
        jumpFrame = true;
        grounded = false;
        velocity.y += jumpForce;
    }
    
    protected void SetMovement(int direction)
    {
        moveDirection = direction;
    }
    
    protected void SetRunning(bool isRunning)
    {
        running = isRunning;
    }
}
