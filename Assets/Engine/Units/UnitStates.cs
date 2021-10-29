using UnityEngine;

public static class UnitStates
{

    public static UnitState Initialise(UnitData data, UnitState state)
    {
        data.t = 0.0f;
        return StateSwitch(data, state, true);
    }

    public static UnitState Execute(UnitData data, UnitState state)
    {
        return StateSwitch(data, state, false);
    }

    private static UnitState StateSwitch(UnitData data, UnitState state, bool initialise)
    {
        switch (state)
        {
            case UnitState.Idle:
                return IdleState(data, initialise);
            case UnitState.Run:
                return RunState(data, initialise);
            case UnitState.Crawl:
                return CrawlState(data, initialise);
            case UnitState.Slide:
                return SlideState(data, initialise);
            case UnitState.Dive:
                return DiveState(data, initialise);
            case UnitState.Jump:
                return JumpState(data, initialise);
            case UnitState.Fall:
                return FallState(data, initialise);
            case UnitState.VaultOverState:
                return VaultOverState(data, initialise);
            case UnitState.VaultOnState:
                return VaultOnState(data, initialise);
            case UnitState.CrawlIdle:
                return CrawlIdleState(data, initialise);
            case UnitState.LedgeGrab:
                return LedgeGrabState(data, initialise);
            case UnitState.WallJump:
                return WallJumpState(data, initialise);
            case UnitState.Climb:
                return ClimbState(data, initialise);
            case UnitState.WallSlide:
                return WallSlideState(data, initialise);
        }
        Debug.LogError("No State Function for " + state.ToString());
        return UnitState.Null;
    }

    private static UnitState IdleState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.animator.Play("Idle");
            data.isStanding = true;
        }

        if (data.t == 0.0f)
        {
            data.ApplyDrag(data.stats.groundDrag);

            Vector2 velocity = data.rb.velocity;
            float desiredSpeed = (data.input.running ? data.stats.runSpeed : data.stats.walkSpeed) * data.input.movement;
            float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
            // Increase acceleration when trying to move in opposite direction of travel
            if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
            {
                deltaSpeedRequired *= 2.0f;
            }
            velocity.x += deltaSpeedRequired * data.stats.groundAcceleration;
            data.rb.velocity = velocity;
            if (Mathf.Abs(data.rb.velocity.x) > data.stats.runSpeed)
            {
                UnitHelper.Instance.EmitGroundParticles(data.rb.position + (Vector2.down * data.stats.standingSpringDistance), data.rb.velocity);
            }
            
            // Execute Jump
            if (data.input.jumpQueued)
            {
                return UnitState.Jump;
            }
            // Execute Run
            if (data.input.movement != 0 && Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed * 0.75f)
            {
                return UnitState.Run;
            }
        }

        // Execute Crawl
        if (data.input.crawling || data.t != 0)
        {
            if (data.t == 0)
            {
                // Play stand to crawl, wait before entering state
                data.animator.Play("StandToCrawl");
                data.animator.Update(0);
                data.animator.Update(0);
                data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
                data.isStanding = false;
            }
            else
            {
                // Waiting to enter crawl state
                data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);

                if (data.t == 0.0f)
                    return UnitState.CrawlIdle;
            }
        }

        return UnitState.Idle;
    }
    
    private static UnitState RunState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            //if (!data.animator.GetCurrentAnimatorStateInfo(0).IsName("Run_Left") && !data.animator.GetCurrentAnimatorStateInfo(0).IsName("Run_Right"))
            //{
                data.animator.Play("Run");
            //}
            data.isStanding = true;
        }

        if (data.t == 0.0f)
        {
            if (Mathf.Abs(data.rb.velocity.x) < data.stats.runSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = (data.input.running ? data.stats.runSpeed : data.stats.walkSpeed) * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * data.stats.groundAcceleration;
                data.rb.velocity = velocity;
            }
            else
            {
                // Apply drag when faster than run speed
                UnitHelper.Instance.EmitGroundParticles(data.rb.position + (Vector2.down * data.stats.standingSpringDistance), data.rb.velocity);
                data.ApplyDrag(data.stats.groundDrag);
            }
            // Check Vault / Climb
            if (Mathf.Abs(data.rb.velocity.x) >= data.stats.walkSpeed * 0.9f)
            {
                UnitState climbState = TryClimb(data);
                if (climbState != UnitState.Null)
                {
                    return climbState;
                }
            }
            if(Mathf.Abs(data.rb.velocity.x) >= data.stats.runSpeed * 0.9f) {
                UnitState vaultState = TryVault(data);
                if (vaultState != UnitState.Null)
                {
                    return vaultState;
                }
            }
            
            // Execute Jump
            if (data.input.jumpQueued)
            {
                return UnitState.Jump;
            }
            // Execute Fall
            if (!data.isGrounded)
            {
                return UnitState.Fall;
            }
            // Return to Idle when below walk speed
            if (Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed * 0.5f)
            {
                return UnitState.Idle;
            }
        }

        if (data.input.crawling || data.t != 0)
        {
            if (data.t == 0)
            {
                if ((data.possibleStates & UnitState.Slide) != 0 && Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed)
                {
                    // Execute Slide
                    data.animator.Play("Slide");
                    return UnitState.Slide;
                }
                else if ((data.possibleStates & UnitState.Crawl) != 0)
                {
                    // Play stand to crawl, wait before entering state
                    data.animator.Play("StandToCrawl");
                    data.animator.Update(0);
                    data.animator.Update(0);
                    data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
                    data.isStanding = false;
                }
            }
            else
            {
                // Waiting to enter crawl state
                data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
                
                if (data.t == 0.0f)
                    return UnitState.Crawl;
            }
        }
        
        return UnitState.Run;
    }

    private static UnitState CrawlIdleState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.isStanding = false;
            data.animator.Play("Crawl_Idle");
        }

        if (data.t == 0.0f)
        {
            // Apply movement input
            if (data.isGrounded && data.rb.velocity.x < data.stats.walkSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = data.stats.walkSpeed * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * data.stats.groundAcceleration;
                data.rb.velocity = velocity;
            }
            if (Mathf.Abs(data.rb.velocity.x) > 0.1f)
            {
                return UnitState.Crawl;
            }
        }

        // Return to Idle
        if ((!data.input.crawling && data.isGrounded) || data.t != 0.0f)
        {
            // Set unit timer to exit animation duration
            if (data.t == 0)
            {
                // Execute animation transition
                data.animator.Play("CrawlToStand");
                // Update animator to transition to relevant state
                data.animator.Update(0);
                data.animator.Update(0);
                data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
                data.isStanding = true;
            }
            else
            {
                data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
                data.ApplyDrag(data.stats.groundDrag);

                if (data.t == 0.0f)
                    return UnitState.Idle;
            }
        }
        
        if (!data.isGrounded) {
            return UnitState.Fall;
        }
        
        return UnitState.CrawlIdle;
    }

    private static UnitState CrawlState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.isStanding = false;
            data.animator.Play("Crawl");
        }

        if (data.t == 0.0f)
        {
            // Apply movement input
            if (data.isGrounded && data.rb.velocity.x < data.stats.walkSpeed)
            {
                Vector2 velocity = data.rb.velocity;
                float desiredSpeed = data.stats.walkSpeed * data.input.movement;
                float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
                // Increase acceleration when trying to move in opposite direction of travel
                if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
                {
                    deltaSpeedRequired *= 2.0f;
                }
                velocity.x += deltaSpeedRequired * data.stats.groundAcceleration;
                data.rb.velocity = velocity;
            }

            if (Mathf.Abs(data.rb.velocity.x) < 0.1f)
            {
                return UnitState.CrawlIdle;
            }
        }

        // Return to Idle
        if ((!data.input.crawling && data.isGrounded) || data.t != 0.0f)
        {
            // Set unit timer to exit animation duration
            if (data.t == 0)
            {
                // Execute animation transition
                data.animator.Play("CrawlToStand");
                // Update animator to transition to relevant state
                data.animator.Update(0);
                data.animator.Update(0);
                data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
                data.isStanding = true;
            }
            // Tick unit timer
            if (data.t != 0.0f)
            {
                data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
                data.ApplyDrag(data.stats.groundDrag);

                if (data.t == 0.0f)
                    return UnitState.Idle;
            }
        }

        if (!data.isGrounded)
        {
            return UnitState.Fall;
        }
        
        return UnitState.Crawl;
    }
    
    private static UnitState SlideState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.isStanding = false;
            if (data.previousState != UnitState.Dive)
            {
                data.rb.velocity *= data.stats.slideVelocityMultiplier;
            }
        }

        if (data.isGrounded || data.t != 0.0f)
        {
            // If we are in the slide loop and let go of crawl input, start a timer to release the slide state
            if (data.input.crawling == false || data.t != 0.0f)
            {
                // Set unit timer to exit animation duration
                if (data.t == 0)
                {
                    // Execute animation transition
                    data.animator.Play(data.previousState == UnitState.Dive ? "DiveFlip" : "SlideExit");
                    // Update animator to transition to relevant state
                    data.animator.Update(0);
                    data.animator.Update(0);
                    data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
                    data.isStanding = true;
                }
                // Tick unit timer
                if (data.t != 0.0f)
                {
                    data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
                    data.ApplyDrag(data.stats.groundDrag);

                    // Execute Jump (only 100ms before returning idle)
                    if (data.t < 0.1f && (data.possibleStates & UnitState.Jump) != 0 && data.input.jumpQueued)
                        return UnitState.Jump;

                    if (data.t == 0.0f)
                        return UnitState.Idle;
                }
            }
            else
            {
                data.ApplyDrag(data.stats.slideDrag);
                // Return to crawl if speed drops too low
                if (data.rb.velocity.magnitude < data.stats.walkSpeed)
                {
                    data.animator.Play("Crawl_Idle");
                    return UnitState.Crawl;
                }
            }
        }
        else
        {
            data.ApplyDrag(data.stats.airDrag);
        }
        
        return UnitState.Slide;
    }
    
    private static UnitState DiveState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Dive");
            data.isStanding = false;
            // Boost when diving from jump
            if (data.previousState == UnitState.Jump || data.previousState == UnitState.WallJump)
            {
                data.rb.velocity *= data.stats.diveVelocityMultiplier;
            }
            // Set timer to stop ground spring
            data.groundSpringActive = false;
            data.t = 0.2f;
        }

        // Allow player to push towards movement speed while in the air
        if (Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
        {
            Vector2 velocity = data.rb.velocity;
            float desiredSpeed = data.stats.walkSpeed * data.input.movement;
            float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
            velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
            data.rb.velocity = velocity;
        }
        
        // Re-enable ground spring after delay
        if(!data.groundSpringActive)
        {
            data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
            if(data.t == 0.0f)
            {
                data.groundSpringActive = true;
            }
        }
        else
        {
            // Have we landed already?
            if (data.isGrounded || data.t != 0.0f)
            {
                // Return to standing on grounded, continue execution until t = 0
                if (!data.input.crawling || data.t != 0.0f)
                {
                    // Set unit timer to exit animation duration
                    if (data.t == 0)
                    {
                        // Execute animation transition
                        data.animator.Play(Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed ? "DiveFlip" : "CrawlToStand");
                        // Update animator to transition to relevant state
                        data.animator.Update(0);
                        data.animator.Update(0);
                        data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
                        data.isStanding = true;
                    }
                    // Tick unit timer
                    if (data.t != 0.0f)
                    {
                        data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
                        data.ApplyDrag(data.stats.groundDrag);

                        // Execute Jump (only 100ms before returning idle)
                        if (data.t < 0.1f && (data.possibleStates & UnitState.Jump) != 0 && data.input.jumpQueued && data.isGrounded)
                        {
                            return UnitState.Jump;
                        }

                        if (data.t == 0.0f && (data.possibleStates & UnitState.Idle) != 0)
                        {
                            data.groundSpringActive = true;
                            return UnitState.Idle;
                        }
                    }
                }
                else
                {
                    data.groundSpringActive = true;
                    // Landed but still crawling, belly slide
                    if (data.rb.velocity.magnitude > data.stats.walkSpeed)
                    {
                        // Execute Slide
                        data.animator.Play("BellySlide");
                        return UnitState.Slide;
                    }
                    else
                    {
                        // Execute Crawl
                        data.animator.Play("Crawl_Idle");
                        return UnitState.Crawl;
                    }
                }
            }
            else
            {
                data.ApplyDrag(data.stats.airDrag);
            }
        }
        return UnitState.Dive;
    }

    private static UnitState JumpState(UnitData data, bool initialise)
    {
        Vector2 velocity = data.rb.velocity;
        if(initialise)
        {
            // Reset jump input
            data.input.jumpRequestTime = -1;
            data.animator.Play("Jump");
            data.animator.Update(0);
            data.animator.Update(0);
            data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
            data.isStanding = true;
            data.groundSpringActive = false;
            velocity.y = data.stats.jumpForce;
        }

        // Check Climb
        if (Mathf.Abs(data.rb.velocity.x) >= 0.1f)
        {
            UnitState climbState = TryClimb(data);
            if (climbState != UnitState.Null)
            {
                return climbState;
            }
        }
        // Check Vault (Require Momentum)
        if (Mathf.Abs(data.rb.velocity.x) >= data.stats.runSpeed * 0.9f)
        {
            UnitState vaultState = TryVault(data);
            if (vaultState != UnitState.Null)
            {
                return vaultState;
            }
        }

        // Wall Slide
        if (!initialise && Mathf.Abs(data.rb.velocity.x) > 0.1f)
        {
            RaycastHit2D wallHit = Physics2D.Raycast(
                data.rb.position,
                data.isFacingRight ? Vector3.right : Vector3.left,
                data.stats.wallDetectionDistance,
                Unit.collisionMask
            );
            Debug.DrawRay(
                data.rb.position,
                (data.isFacingRight ? Vector3.right : Vector3.left) * data.stats.wallDetectionDistance,
                wallHit ? Color.green : Color.red
            );
            if (wallHit)
            {
                return UnitState.WallSlide;
            }
        }

        // Allow player to push towards movement speed while in the air
        if (Mathf.Abs(data.rb.velocity.x) < data.stats.runSpeed)
        {
            float desiredSpeed = (data.input.running ? data.stats.runSpeed : data.stats.walkSpeed) * data.input.movement;
            float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
            velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
        }
        
        if (!data.isGrounded || !data.groundSpringActive)
        {
            // Execute Dive
            if ((data.possibleStates & UnitState.Dive) != 0 && data.input.crawling)
            {
                return UnitState.Dive;
            }
        }
        
        // End of jump animation
        if (data.t == 0.0f)
        {
            data.groundSpringActive = true;
            return data.isGrounded ? UnitState.Idle : UnitState.Fall;
        }

        data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
        data.rb.velocity = velocity;
        return UnitState.Jump;
    }

    private static UnitState FallState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.t = 0.1f;
            data.isStanding = true;
            data.groundSpringActive = true;
        }

        if(data.t > 0.0f)
        {
            data.t -= Time.deltaTime;
            // Check Vault
            if (Mathf.Abs(data.rb.velocity.x) >= data.stats.runSpeed * 0.9f)
            {
                UnitState vaultState = TryVault(data);
                if (vaultState != UnitState.Null)
                {
                    return vaultState;
                }
            }
        }

        if(data.t <= 0.0f && data.t != -10.0f)
        {
            data.animator.Play("Fall");
            data.t = -10.0f;
        }

        // Allow player to push towards movement speed while in the air
        if (Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
        {
            Vector2 velocity = data.rb.velocity;
            float desiredSpeed = data.stats.walkSpeed * data.input.movement;
            float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
            velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
            data.rb.velocity = velocity;
        }
        
        // Return to ground
        if (data.isGrounded)
        {
            if (Mathf.Abs(data.rb.velocity.x) > 0.1f)
            {
                return UnitState.Run;
            }
            else
            {
                return UnitState.Idle;
            }
        }


        // Check Climb
        UnitState climbState = TryClimb(data);
        if (climbState != UnitState.Null)
        {
            return climbState;
        }
        
        // Wall Slide
        RaycastHit2D wallHit = Physics2D.Raycast(
            data.rb.position,
            data.isFacingRight ? Vector3.right : Vector3.left,
            data.stats.wallDetectionDistance,
            Unit.collisionMask
        );
        Debug.DrawRay(
            data.rb.position,
            (data.isFacingRight ? Vector3.right : Vector3.left) * data.stats.wallDetectionDistance,
            wallHit ? Color.green : Color.red
        );
        if (wallHit)
        {
            return UnitState.WallSlide;
        }

        // Execute Dive
        if ((data.possibleStates & UnitState.Dive) != 0 && data.input.crawling)
        {
            return UnitState.Dive;
        }

        return UnitState.Fall;
    }
    
    private static UnitState VaultOverState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("VaultOver");
            data.t = data.stats.vaultDuration;
            data.isStanding = true;
            data.groundSpringActive = false;
            // Disable collider
            data.rb.bodyType = RigidbodyType2D.Kinematic;
            data.target = data.target - data.rb.position;
            Debug.DrawRay(data.rb.position, data.target, Color.blue, data.t);
        }

        if (data.t > 0.0f)
        {
            data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
            data.rb.velocity = (data.target / data.stats.vaultDuration);
        }
        if(data.t == 0.0f)
        {
            data.groundSpringActive = true;
            // Enable collider
            data.rb.bodyType = RigidbodyType2D.Dynamic;
            return UnitState.Idle;
        }
        return UnitState.VaultOverState;
    }
    
    private static UnitState VaultOnState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.animator.Play("VaultOn");
            data.t = data.stats.vaultDuration;
            data.isStanding = true;
            data.groundSpringActive = false;
            // Disable collider
            data.rb.bodyType = RigidbodyType2D.Kinematic;
            data.target = data.target - data.rb.position;
            Debug.DrawRay(data.rb.position, data.target, Color.blue, data.t);
        }

        if (data.t > 0.0f)
        {
            data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
            data.rb.velocity = (data.target / data.stats.vaultDuration);
        }
        if (data.t == 0.0f)
        {
            data.groundSpringActive = true;
            // Enable collider
            data.rb.bodyType = RigidbodyType2D.Dynamic;
            return UnitState.Idle;
        }
        return UnitState.VaultOnState;
    }
    
    private static UnitState LedgeGrabState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("LedgeGrab");
            data.t = 0.1f;
            data.groundSpringActive = false;
            data.updateFacing = false;
        }

        data.t -= Time.fixedDeltaTime;

        if (data.t > 0.0f) {
            data.rb.velocity = (data.target - data.rb.position) / data.t;
        }

        if (data.t <= 0.0f && data.t != -10.0f) {
            data.rb.position = data.target;
            data.rb.velocity = Vector2.zero;
            data.updateFacing = true;
            data.t = -10.0f;
        }
        
        if (data.t == -10.0f) {
            // Wall Jump
            if (data.input.jumpQueued)
            {
                return UnitState.WallJump;
            }

            // Climb Right
            if (data.isFacingRight && data.input.movement > 0)
            {
                return UnitState.Climb;
            }
            // Climb Left
            if (!data.isFacingRight && data.input.movement < 0)
            {
                return UnitState.Climb;
            }

            // Jump Away Right
            if (!data.isFacingRight && data.input.movement > 0)
            {
                return UnitState.WallJump;
            }
            // Jump Away Left
            if (data.isFacingRight && data.input.movement < 0)
            {
                return UnitState.WallJump;
            }

            // Drop
            if (data.input.crawling)
            {
                return UnitState.WallSlide;
            }
        }
        
        return UnitState.LedgeGrab;
    }
    
    private static UnitState WallJumpState(UnitData data, bool initialise)
    {        
        if(initialise)
        {
            data.isFacingRight = !data.isFacingRight;
            data.animator.SetBool("FacingRight", data.isFacingRight);
            data.animator.Play("WallJump");
            data.animator.Update(0);
            data.animator.Update(0);
            data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
            data.rb.velocity = (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.wallJumpForce.x +
                                Vector2.up * data.stats.wallJumpForce.y;
        }
        
        data.t = Mathf.Max(0, data.t - Time.fixedDeltaTime);
        if(data.t == 0.0f)
        {
            data.groundSpringActive = true;
            return UnitState.Fall;
        }

        // Try grabbing another ledge
        if (Mathf.Abs(data.rb.velocity.x) >= data.stats.walkSpeed * 0.9f)
        {
            UnitState climbState = TryClimb(data);
            if (climbState != UnitState.Null)
            {
                return climbState;
            }
        }

        // Execute Dive
        if (data.input.crawling)
        {
            return UnitState.Dive;
        }
        
        return UnitState.WallJump;
    }
    
    private static UnitState ClimbState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Climb");
            Debug.DrawLine(
                data.rb.position,
                data.rb.position + (Vector2.up * (data.stats.standingSpringDistance - data.stats.climbGrabOffset.y)) + ((data.isFacingRight ? Vector2.right : Vector2.left) * (data.stats.standingSpringWidth - data.stats.climbGrabOffset.x)),
                Color.blue,
                data.stats.climbDuration
            );
            data.t = data.stats.climbDuration;
            data.target = data.rb.position + (Vector2.up * (data.stats.standingSpringDistance - data.stats.climbGrabOffset.y)) + ((data.isFacingRight ? Vector2.left : Vector2.right) * data.stats.climbGrabOffset.x);
            data.rb.velocity = (data.target - data.rb.position) / data.t;
            data.rb.isKinematic = true;
        }

        data.t = Mathf.Max(0, data.t - Time.fixedDeltaTime);
        if(data.t == 0.0f) {
            data.rb.velocity = Vector2.zero;
            data.rb.position = data.target;
            data.rb.isKinematic = false;
            data.groundSpringActive = true;
            return UnitState.Idle;
        } else {
            data.rb.velocity = (data.target - data.rb.position) / data.t;
        }
        
        return UnitState.Climb;
    }
    
    private static UnitState WallSlideState(UnitData data, bool initialise)
    {
        if (initialise) {
            data.animator.Play("WallSlide");
            data.updateFacing = false;
        }

        // Check we are still on a wall
        RaycastHit2D wallHit = Physics2D.Raycast(
            data.rb.position,
            data.isFacingRight ? Vector3.right : Vector3.left,
            data.stats.wallDetectionDistance,
            Unit.collisionMask
        );
        Debug.DrawRay(
            data.rb.position,
            (data.isFacingRight ? Vector3.right : Vector3.left) * data.stats.wallDetectionDistance,
            wallHit ? Color.green : Color.red
        );
        if (!wallHit)
        {
            data.updateFacing = true;
            return UnitState.Fall;
        }
        
        if (data.rb.velocity.y <= 0.0f) {
            data.groundSpringActive = true;
        }
        
        // Wall Jump
        if (data.rb.velocity.y > 0)
        {
            if (data.input.jumpQueued)
            {
                data.updateFacing = true;
                return UnitState.WallJump;
            }
            // Jump Away Right
            if (!data.isFacingRight && data.input.movement > 0)
            {
                data.updateFacing = true;
                return UnitState.WallJump;
            }
            // Jump Away Left
            if (data.isFacingRight && data.input.movement < 0)
            {
                data.updateFacing = true;
                return UnitState.WallJump;
            }
        }

        // Grab Ledge
        UnitState climbState = TryClimb(data);
        if (climbState != UnitState.Null) {
            return climbState;
        }

        if (data.isGrounded)
        {
            data.updateFacing = true;
            return UnitState.Idle;
        }
        return UnitState.WallSlide;
    }
    
    #region Helpers
    
    private static UnitState TryVault(UnitData data)
    {
        const float nearHitBuffer = 0.25f;
        RaycastHit2D nearHit = Physics2D.BoxCast(
            data.rb.position + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)) + (Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f),
            new Vector2(data.stats.vaultGrabDistance - nearHitBuffer, data.stats.maxVaultHeight - data.stats.minVaultHeight),
            data.rb.rotation,
            data.isFacingRight ? Vector2.right : Vector2.left,
            data.stats.vaultGrabDistance * 0.5f,
            Unit.collisionMask
        );
        ExtDebug.DrawBoxCastOnHit(
            data.rb.position + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)) + (Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f),
            new Vector2(data.stats.vaultGrabDistance - nearHitBuffer, data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f,
            Quaternion.Euler(0, 0, data.rb.rotation),
            data.isFacingRight ? Vector2.right : Vector2.left,
            data.stats.vaultGrabDistance * 0.5f,
            nearHit ? Color.green : Color.red
        );
        if (nearHit) { return UnitState.Null; }

        RaycastHit2D vaultHit = Physics2D.Raycast(
            data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultGrabDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
            Vector2.down,
            data.stats.maxVaultHeight - data.stats.minVaultHeight
        );
        Debug.DrawRay(
            data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultGrabDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
            Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
            Color.red
        );
        if(vaultHit)
        {
            Debug.DrawRay(
                data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultGrabDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
                Vector2.down * vaultHit.distance,
                Color.green,
                data.stats.vaultDuration
            );
            // Dont vault if surface is not flat
            if (Vector2.Dot(Vector2.up, vaultHit.normal) <= 0.9f) {
                Debug.DrawRay(vaultHit.point, vaultHit.normal, Color.red, data.stats.vaultDuration);
                return UnitState.Null;
            }
            // Dont vault if ray is inside a collider
            if (vaultHit.distance > 0.0f)
            {
                // Vault over object or on top of it
                RaycastHit2D landingZoneHit = Physics2D.Raycast(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
                    Vector2.down,
                    data.stats.maxVaultHeight - data.stats.minVaultHeight
                );
                Debug.DrawRay(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
                    Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
                    Color.red,
                    data.stats.vaultDuration
                );
                if(landingZoneHit)
                {
                    // Landing zone obstruction
                    Debug.DrawRay(
                        data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
                        Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
                        Color.green,
                        data.stats.vaultDuration
                    );
                    data.target = data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.up * data.stats.standingSpringDistance * 0.5f);
                    return UnitState.VaultOnState;
                }
                else
                {
                    // Landing zone clear
                    data.target = data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance);
                    return UnitState.VaultOverState;
                }
            }
        }
        return UnitState.Null;
    }

    private static UnitState TryClimb(UnitData data)
    {
        RaycastHit2D climbHit = Physics2D.Raycast(
            data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.climbGrabDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxClimbHeight)),
            Vector2.down,
            data.stats.maxClimbHeight - data.stats.minClimbHeight
        );
        Debug.DrawRay(
            data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.climbGrabDistance) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxClimbHeight)),
            Vector2.down * (data.stats.maxClimbHeight - data.stats.minClimbHeight),
            Color.red
        );

        if (climbHit && climbHit.distance > 0.0f && Vector2.Dot(Vector2.up, climbHit.normal) >= 0.9f)
        {
            // Scan for nearest edge
            bool hitPlatform = true;
            float inset = 0.0f;
            while (hitPlatform) {
                inset += 0.1f;
                RaycastHit2D edgeScan = Physics2D.Raycast(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * (data.stats.climbGrabDistance - inset)) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxClimbHeight)),
                    Vector2.down,
                    data.stats.maxClimbHeight - data.stats.minClimbHeight
                );
                Debug.DrawRay(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * (data.stats.climbGrabDistance - inset)) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxClimbHeight)),
                    Vector2.down * (data.stats.maxClimbHeight - data.stats.minClimbHeight),
                    Color.red,
                    data.stats.climbDuration
                );
                if(edgeScan && Vector2.Dot(Vector2.up, edgeScan.normal) >= 0.9f) {
                    hitPlatform = true;
                    climbHit = edgeScan;
                } else {
                    inset -= 0.1f;
                    hitPlatform = false;
                }
            }
            
            Debug.DrawRay(
                data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * (data.stats.climbGrabDistance - inset)) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxClimbHeight)),
                Vector2.down * climbHit.distance,
                Color.green,
                data.stats.climbDuration
            );
            
            data.target = climbHit.point + (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.climbGrabOffset.x + Vector2.up * data.stats.climbGrabOffset.y;
            return UnitState.LedgeGrab;
        }
        return UnitState.Null;
    }
    
    #endregion
}
