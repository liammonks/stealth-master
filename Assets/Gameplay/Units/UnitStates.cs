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
            case UnitState.Null:
                return NullState(data, initialise);
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
            case UnitState.Melee:
                return MeleeState(data, initialise);
            case UnitState.JumpMelee:
                return JumpMeleeState(data, initialise);
            case UnitState.GrappleHookSwing:
                return GrappleHookSwingState(data, initialise);
            case UnitState.HitImpact:
                return HitImpactState(data, initialise);
            case UnitState.Launched:
                return LaunchedState(data, initialise);
        }
        Debug.LogError("No State Function for " + state.ToString());
        return UnitState.Null;
    }
    
    private static UnitState NullState(UnitData data, bool initialise)
    {
        data.ApplyDrag(data.isGrounded ? data.stats.groundDrag : data.stats.airDrag);
        return UnitState.Null;
    }

    private static UnitState GrappleHookSwingState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Fall");
        }

        Vector2 velocity = data.rb.velocity;
        float desiredSpeed = (data.input.running ? data.stats.runSpeed : data.stats.walkSpeed) * data.input.movement;
        if (Mathf.Abs(desiredSpeed) > Mathf.Abs(velocity.x))
        {
            float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
            // Increase acceleration when trying to move in opposite direction of travel
            if ((desiredSpeed < -0.1f && velocity.x > 0.1f) || (desiredSpeed > 0.1f && velocity.x < -0.1f))
            {
                deltaSpeedRequired *= 2.0f;
            }
            velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
            data.rb.velocity = velocity;
        }

        data.ApplyDrag(data.stats.airDrag);
        return UnitState.GrappleHookSwing;
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
                UnitHelper.Instance.EmitGroundParticles(data.rb.position + (Vector2.down * data.stats.standingHalfHeight), data.rb.velocity);
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
            // Execute Fall
            if (!data.isGrounded)
            {
                return UnitState.Fall;
            }
            // Push against wall
            if (FacingWall(data))
            {
                data.animator.Play("AgainstWall");
            }
            else
            {
                data.animator.Play("Idle");
            }
            // Execute Melee
            if(data.input.meleeQueued)
            {
                return UnitState.Melee;
            }
        }

        // Execute Crawl
        if (data.t != 0 || (data.input.crawling && CanCrawl(data)))
        {
            if (data.t == 0)
            {
                // Play stand to crawl, wait before entering state
                data.animator.Play("StandToCrawl");
                //data.animator.Update(0);
                //data.animator.Update(0);
                data.t = data.animator.GetState().length;
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
        UpdateFacing(data);
        return UnitState.Idle;
    }
    
    private static UnitState RunState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Run");
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
                UnitHelper.Instance.EmitGroundParticles(data.rb.position + (Vector2.down * data.stats.standingHalfHeight), data.rb.velocity);
                data.ApplyDrag(data.stats.groundDrag);
            }
            // Check Vault / Climb
            if (Mathf.Abs(data.rb.velocity.x) >= data.stats.walkSpeed * 0.9f)
            {
                UnitState climbState = TryLedgeGrab(data);
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
            // Face Wall
            if (FacingWall(data))
            {
                data.animator.Play("AgainstWall");
            }
            else
            {
                data.animator.Play("Run");
            }
            // Execute Melee
            if (data.input.meleeQueued)
            {
                return UnitState.Melee;
            }
        }

        if (data.t != 0 || (data.input.crawling && CanCrawl(data)))
        {
            if (data.t == 0)
            {
                if (Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed)
                {
                    // Execute Slide
                    data.animator.Play("Slide");
                    return UnitState.Slide;
                }
                else
                {
                    // Play stand to crawl, wait before entering state
                    data.animator.Play("StandToCrawl");
                    //data.animator.Update(0);
                    //data.animator.Update(0);
                    data.t = data.animator.GetState().length;
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
        UpdateFacing(data);
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
        if (data.t != 0.0f || (!data.input.crawling && data.isGrounded && CanStand(data, data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f))))
        {
            // Set unit timer to exit animation duration
            if (data.t == 0)
            {
                // Execute animation transition
                data.animator.Play("CrawlToStand");
                // Update animator to transition to relevant state
                //data.animator.Update(0);
                //data.animator.Update(0);
                data.t = data.animator.GetState().length;
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
        
        if(!data.isGrounded) {
            return UnitState.Dive;
        }
        UpdateFacing(data);
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
        
        if(!data.isGrounded) {
            // Grab on to ledges below
            UnitState ledgeDrop = TryDrop(data);
            if (ledgeDrop != UnitState.Null)
            {
                data.input.crawling = false;
                data.input.crawlRequestTime = -1;
                return ledgeDrop;
            }
        }

        // Return to Idle
        if (data.t != 0.0f || (!data.input.crawling && data.isGrounded && CanStand(data, data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f))))
        {
            // Set unit timer to exit animation duration
            if (data.t == 0)
            {
                // Execute animation transition
                data.animator.Play("CrawlToStand");
                // Update animator to transition to relevant state
                //data.animator.Update(0);
                //data.animator.Update(0);
                data.t = data.animator.GetState().length;
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
        UpdateFacing(data);
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
            if (data.t != 0.0f || (!data.input.crawling && CanStand(data, data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f))))
            {
                // Set unit timer to exit animation duration
                if (data.t == 0)
                {
                    // Execute animation transition
                    data.animator.Play(data.previousState == UnitState.Dive ? "DiveFlip" : "SlideExit");
                    // Update animator to transition to relevant state
                    //data.animator.Update(0);
                    //data.animator.Update(0);
                    data.t = data.animator.GetState().length;
                    data.isStanding = true;
                }
                // Tick unit timer
                if (data.t != 0.0f)
                {
                    data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
                    data.ApplyDrag(data.stats.groundDrag);

                    // Execute Jump (only 100ms before returning idle)
                    if (data.t < 0.1f && data.input.jumpQueued)
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

        if (data.stateDuration <= 0.3f && !data.isGrounded && data.previousState != UnitState.Dive)
        {
            // Grab on to ledges below
            UnitState ledgeDrop = TryDrop(data);
            if (ledgeDrop != UnitState.Null)
            {
                data.input.crawling = false;
                data.input.crawlRequestTime = -1;
                return ledgeDrop;
            }
        }
        UpdateFacing(data);
        return UnitState.Slide;
    }
    
    private static UnitState DiveState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            if (data.previousState == UnitState.Crawl || data.previousState == UnitState.CrawlIdle) {
                data.animator.Play("BellySlide");
            } else {
                data.animator.Play("Dive");
            }
            data.isStanding = false;
            // Boost when diving from jump
            if (data.previousState == UnitState.Jump || data.previousState == UnitState.WallJump)
            {
                //data.rb.velocity *= data.stats.diveVelocityMultiplier;
                Vector2 velocity = data.rb.velocity;
                velocity.x += 5 * Mathf.Sign(data.rb.velocity.x);
                velocity.y += 2 * Mathf.Sign(data.rb.velocity.y);
                data.rb.velocity = velocity;
            }
            // Set timer to stop ground spring
            data.groundSpringActive = false;
            data.t = 0.2f;
        }

        // Allow player to push towards movement speed while in the air
        if (!data.isSlipping && Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
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
                if (data.t != 0.0f || (!data.input.crawling && CanStand(data, data.rb.transform.up * (-data.stats.crawlingHalfHeight + data.stats.standingHalfHeight + 0.01f))))
                {
                    // Set unit timer to exit animation duration
                    if (data.t == 0)
                    {
                        // Execute animation transition
                        data.animator.Play(Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed ? "DiveFlip" : "CrawlToStand");
                        // Update animator to transition to relevant state
                        //data.animator.Update(0);
                        //data.animator.Update(0);
                        data.t = data.animator.GetState().length;
                        data.isStanding = true;
                    }
                    // Tick unit timer
                    if (data.t != 0.0f)
                    {
                        data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
                        data.ApplyDrag(data.stats.groundDrag);

                        // Execute Jump (only 100ms before returning idle)
                        if (data.t < 0.1f && data.input.jumpQueued && data.isGrounded)
                        {
                            return UnitState.Jump;
                        }

                        if (data.t == 0.0f)
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
        UpdateFacing(data);
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
            //data.animator.Update(0);
            //data.animator.Update(0);
            data.t = data.animator.GetState().length;
            data.isStanding = true;
            data.groundSpringActive = false;
            velocity.y = data.previousState == UnitState.LedgeGrab ? data.stats.wallJumpForce.y : data.stats.jumpForce;
        }

        // Check Climb
        if (Mathf.Abs(data.rb.velocity.x) >= 0.1f)
        {
            UnitState climbState = TryLedgeGrab(data);
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
        if (!initialise && Mathf.Abs(data.rb.velocity.x) > 0.1f && FacingWall(data))
        {
            return UnitState.WallSlide;
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
            if (data.input.crawling && CanCrawl(data))
            {
                return UnitState.Dive;
            }
        }
        
        // Melee
        if(data.input.meleeQueued)
        {
            return UnitState.JumpMelee;
        }
        
        // End of jump animation
        if (data.t == 0.0f)
        {
            data.groundSpringActive = true;
            return data.isGrounded ? UnitState.Idle : UnitState.Fall;
        }

        data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
        data.rb.velocity = velocity;
        UpdateFacing(data);
        return UnitState.Jump;
    }

    private static UnitState FallState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.t = 0.1f;
            data.isStanding = true;
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
        
        if(data.rb.velocity.y < 0) {
            data.groundSpringActive = true;
        }

        // Allow player to push towards movement speed while in the air
        if (!data.isSlipping && Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
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
        UnitState climbState = TryLedgeGrab(data);
        if (climbState != UnitState.Null)
        {
            return climbState;
        }
        
        // Wall Slide
        if(FacingWall(data)) {
            return UnitState.WallSlide;
        }

        // Execute Dive
        if (data.input.crawling && data.previousState != UnitState.WallSlide && CanCrawl(data))
        {
            return UnitState.Dive;
        }
        UpdateFacing(data);
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
            // Check if the ledge wall extends down to feet
            RaycastHit2D feetHit = Physics2D.Raycast(
                data.target + (Vector2.down * data.stats.standingScale * 0.4f),
                data.isFacingRight ? Vector2.right : Vector2.left,
                data.stats.standingScale.x * 0.6f,
                Unit.CollisionMask
            );
            Debug.DrawRay(
                data.target + (Vector2.down * data.stats.standingScale * 0.4f),
                (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.6f,
                feetHit ? Color.green : Color.red,
                1.0f
            );
            
            data.animator.Play(feetHit ? "LedgeGrab" : "LedgeGrab_Hang");
            data.t = 0.2f;
            data.groundSpringActive = false;
            data.isStanding = true;
        }

        data.rb.rotation = 0;
        data.t -= Time.fixedDeltaTime;

        if (data.t > 0.0f) {
            data.rb.velocity = (data.target - data.rb.position) / Mathf.Max(0.01f, data.t);
        }

        if (data.t <= 0.0f && data.t != -10.0f) {
            data.rb.position = data.target;
            data.rb.velocity = Vector2.zero;
            data.t = -10.0f;
        }

        if (data.t == -10.0f) {
            // Wall Jump
            if (data.input.jumpQueued)
            {
                return UnitState.Jump;
            }
            // Climb Right
            if (data.isFacingRight && data.input.movement > 0)
            {
                if (CanClimb(data))
                {
                    return UnitState.Climb;
                }
            }
            // Climb Left
            if (!data.isFacingRight && data.input.movement < 0)
            {
                if (CanClimb(data))
                {
                    return UnitState.Climb;
                }
            }
            // Jump Right
            if (!data.isFacingRight && data.input.movement > 0 && CanStand(data, new Vector2(data.stats.standingHalfWidth, 0)))
            {
                return UnitState.WallJump;
            }
            // Jump Left
            if (data.isFacingRight && data.input.movement < 0 && CanStand(data, new Vector2(-data.stats.standingHalfWidth, 0)))
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
            // Flip facing
            data.isFacingRight = !data.isFacingRight;
            data.animator.SetFacing(data.isFacingRight);
            data.animator.Play("WallJump");
            //data.animator.Update(0);
            //data.animator.Update(0);
            data.t = data.animator.GetState().length;
            data.rb.velocity = (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.wallJumpForce.x +
                                Vector2.up * data.stats.wallJumpForce.y;
        }
        
        data.t = Mathf.Max(0, data.t - Time.fixedDeltaTime);
        if(data.t == 0.0f && data.rb.velocity.y < 0)
        {
            data.animator.Play("WallJumpFall");
            data.groundSpringActive = true;
            return UnitState.Fall;
        }

        // Try grabbing another ledge
        if (Mathf.Abs(data.rb.velocity.x) >= data.stats.walkSpeed * 0.9f)
        {
            UnitState climbState = TryLedgeGrab(data);
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

        // Wall Slide
        if (FacingWall(data))
        {
            return UnitState.WallSlide;
        }
        UpdateFacing(data);
        return UnitState.WallJump;
    }
    
    private static UnitState ClimbState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Climb");
            data.t = data.stats.climbDuration;
            data.target = data.rb.position + (Vector2.up * (data.stats.standingHalfHeight - data.stats.climbGrabOffset.y)) + ((data.isFacingRight ? Vector2.left : Vector2.right) * data.stats.climbGrabOffset.x);
            data.rb.velocity = (data.target - data.rb.position) / data.t;
            data.rb.isKinematic = true;
            Debug.DrawLine(
                data.rb.position,
                data.target,
                Color.blue,
                data.stats.climbDuration
            );
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
        }

        // Check we are still on a wall
        if (!FacingWall(data))
        {
            return UnitState.Fall;
        }
        
        if (data.rb.velocity.y <= 0.0f) {
            data.groundSpringActive = true;
        }
        
        // Wall Jump
        if (data.rb.velocity.y > 0)
        {
            if (data.input.jumpQueued && CanStand(data, new Vector2(data.isFacingRight ? -data.stats.standingHalfWidth : data.stats.standingHalfHeight, 0)))
            {
                return UnitState.WallJump;
            }
            // Jump Away Right
            if (!data.isFacingRight && data.input.movement > 0 && CanStand(data, new Vector2(data.stats.standingHalfWidth, 0)))
            {
                return UnitState.WallJump;
            }
            // Jump Away Left
            if (data.isFacingRight && data.input.movement < 0 && CanStand(data, new Vector2(-data.stats.standingHalfWidth, 0)))
            {
                return UnitState.WallJump;
            }
        }

        if (data.isGrounded)
        {
            return UnitState.Idle;
        }

        // Grab Ledge
        UnitState climbState = TryLedgeGrab(data);
        if (climbState != UnitState.Null)
        {
            return climbState;
        }
        
        return UnitState.WallSlide;
    }
    
    private static UnitState MeleeState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Melee");
            //data.animator.Update(0);
            //data.animator.Update(0);
            data.t = data.animator.GetState().length;
            data.rb.velocity = Vector2.zero;
            data.hitIDs.Clear();
        }
        data.t = Mathf.Max(0.0f, data.t - Time.deltaTime);
        
        if(data.t <= data.animator.GetState().length * 0.5f)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                data.rb.position + data.stats.meleeOffset,
                data.stats.meleeScale,
                data.rb.rotation,
                Vector2.zero,
                0,
                data.hitMask
            );
            ExtDebug.DrawBox(
                data.rb.position + data.stats.meleeOffset,
                data.stats.meleeScale * 0.5f,
                Quaternion.Euler(0, 0, data.rb.rotation),
                hits.Length > 0 ? Color.green : Color.red
            );
            foreach(RaycastHit2D hit in hits)
            {
                Unit unit = hit.rigidbody?.GetComponent<Unit>();
                if (unit && !data.hitIDs.Contains(unit.ID))
                {
                    unit.TakeDamage(
                        data.stats.meleeDamage,
                        data.rb.velocity + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.meleeKnockback * data.stats.knockbackMultiplier)
                    );
                    data.hitIDs.Add(unit.ID);
                }
            }
        }
        
        if(data.t == 0.0f)
        {
            return UnitState.Idle;
        }
        return UnitState.Melee;
    }

    private static UnitState JumpMeleeState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.animator.Play("JumpMelee");
            //data.animator.Update(0);
            //data.animator.Update(0);
            data.t = data.animator.GetState().length;
            data.hitIDs.Clear();
        }
        data.t = Mathf.Max(0.0f, data.t - Time.deltaTime);

        if (data.t <= data.animator.GetState().length * 0.5f)
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                data.rb.position + (Vector2.up * data.stats.jumpMeleeOffset.y) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.jumpMeleeOffset.x),
                data.stats.jumpMeleeScale,
                data.rb.rotation,
                Vector2.zero,
                0,
                data.hitMask
            );
            ExtDebug.DrawBox(
                data.rb.position + (Vector2.up * data.stats.jumpMeleeOffset.y) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.jumpMeleeOffset.x),
                data.stats.jumpMeleeScale * 0.5f,
                Quaternion.Euler(0, 0, data.rb.rotation),
                hits.Length > 0 ? Color.green : Color.red
            );
            foreach (RaycastHit2D hit in hits)
            {
                Unit unit = hit.rigidbody?.GetComponent<Unit>();
                if (unit && !data.hitIDs.Contains(unit.ID))
                {
                    unit.TakeDamage(
                        data.stats.jumpMeleeDamage,
                        data.rb.velocity + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.jumpMeleeKnockback * data.stats.knockbackMultiplier)
                    );
                    data.hitIDs.Add(unit.ID);
                }
            }
        }

        if (data.t == 0.0f)
        {
            return UnitState.Idle;
        }
        return UnitState.JumpMelee;
    }

    private static UnitState HitImpactState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Hit_Impact");
            //data.animator.Update(0);
            //data.animator.Update(0);
            data.t = data.animator.GetState().length;
        }
        data.ApplyDrag(data.isGrounded ? data.stats.groundDrag : data.stats.airDrag);
        data.t = Mathf.Max(data.t - Time.fixedDeltaTime, 0.0f);
        if(data.t == 0.0f)
        {
            return UnitState.Idle;
        }
        return UnitState.HitImpact;
    }

    private static UnitState LaunchedState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.animator.Play("Launched");
            //data.animator.Update(0);
            //data.animator.Update(0);
            data.t = data.animator.GetState().length;
        }
        data.ApplyDrag(data.isGrounded ? data.stats.groundDrag : data.stats.airDrag);
        data.t = Mathf.Max(data.t - Time.fixedDeltaTime, 0.0f);
        if (data.t == 0.0f)
        {
            return UnitState.Idle;
        }
        return UnitState.Launched;
    }

    #region Helpers

    private static void UpdateFacing(UnitData data)
    {
        data.animator.SetVelocity(data.rb.velocity.x);

        if (data.rb.velocity.x > 0.1f) { data.isFacingRight = true; }
        else if (data.rb.velocity.x < -0.1f) { data.isFacingRight = false; }
        else if (data.input.movement > 0.0f) { data.isFacingRight = true; }
        else if (data.input.movement < 0.0f) { data.isFacingRight = false; }
        data.animator.SetFacing(data.isFacingRight);
    }
    
    private static UnitState TryVault(UnitData data)
    {
        const float nearHitBuffer = 0.25f;
        RaycastHit2D nearHit = Physics2D.BoxCast(
            data.rb.position + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)) + (-(Vector2)data.rb.transform.up * (data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f),
            new Vector2(data.stats.vaultGrabDistance - nearHitBuffer, data.stats.maxVaultHeight - data.stats.minVaultHeight),
            data.rb.rotation,
            data.isFacingRight ? Vector2.right : Vector2.left,
            data.stats.vaultGrabDistance * 0.5f,
            Unit.CollisionMask
        );
        ExtDebug.DrawBoxCastOnHit(
            data.rb.position + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)) + (-(Vector2)data.rb.transform.up * (data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f),
            new Vector2(data.stats.vaultGrabDistance - nearHitBuffer, data.stats.maxVaultHeight - data.stats.minVaultHeight) * 0.5f,
            Quaternion.Euler(0, 0, data.rb.rotation),
            data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right,
            data.stats.vaultGrabDistance * 0.5f,
            nearHit ? Color.green : Color.red
        );
        if (nearHit) { return UnitState.Null; }

        RaycastHit2D vaultHit = Physics2D.Raycast(
            data.rb.position + ((data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right) * data.stats.vaultGrabDistance) + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
            -(Vector2)data.rb.transform.up,
            data.stats.maxVaultHeight - data.stats.minVaultHeight
        );
        Debug.DrawRay(
            data.rb.position + ((data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right) * data.stats.vaultGrabDistance) + (-(Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
            -(Vector2)data.rb.transform.up * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
            Color.red
        );
        if(vaultHit)
        {
            Debug.DrawRay(
                data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultGrabDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
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
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
                    Vector2.down,
                    data.stats.maxVaultHeight - data.stats.minVaultHeight
                );
                Debug.DrawRay(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
                    Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
                    Color.red,
                    data.stats.vaultDuration
                );
                if(landingZoneHit)
                {
                    // Landing zone obstruction, try to vault on top of the object
                    Debug.DrawRay(
                        data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.down * (data.stats.standingHalfHeight - data.stats.maxVaultHeight)),
                        Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
                        Color.green,
                        data.stats.vaultDuration
                    );
                    data.target = ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.up * data.stats.standingHalfHeight * 0.5f);
                    // Check if the target area is clear for standing
                    landingZoneHit = Physics2D.BoxCast(
                        data.rb.position + data.target + (Vector2.up * data.stats.standingHalfHeight * 0.5f) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f),
                        data.stats.standingScale,
                        0,
                        Vector2.zero,
                        0,
                        Unit.CollisionMask
                    );
                    ExtDebug.DrawBox(
                        data.rb.position + data.target + (Vector2.up * data.stats.standingHalfHeight * 0.5f) + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f),
                        data.stats.standingScale * 0.5f,
                        Quaternion.identity,
                        landingZoneHit ? Color.green : Color.red,
                        data.stats.vaultDuration
                    );
                    if (!landingZoneHit)
                    {
                        return UnitState.VaultOnState;
                    }
                }
                else
                {
                    const float groundClearance = 0.25f;
                    // Landing zone clear, try to vault over
                    data.target = ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance);
                    landingZoneHit = Physics2D.BoxCast(
                        data.rb.position + data.target + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f) + (Vector2.up * groundClearance),
                        new Vector2(data.stats.standingScale.x, data.stats.standingScale.y - groundClearance),
                        0,
                        Vector2.zero,
                        0,
                        Unit.CollisionMask
                    );
                    ExtDebug.DrawBox(
                        data.rb.position + data.target + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.standingScale.x * 0.5f) + (Vector2.up * groundClearance),
                        new Vector2(data.stats.standingScale.x, data.stats.standingScale.y - groundClearance) * 0.5f,
                        Quaternion.identity,
                        landingZoneHit ? Color.green : Color.red,
                        data.stats.vaultDuration
                    );
                    if (!landingZoneHit)
                    {
                        return UnitState.VaultOverState;
                    }
                }
            }
        }
        return UnitState.Null;
    }

    private static UnitState TryLedgeGrab(UnitData data)
    {
        float minLedgeThickness = 0.1f;
        float scanHeight = data.stats.maxClimbHeight - data.stats.minClimbHeight;
        float scanHeightInterval = 0.01f;
        const float boxDepth = 0.1f;
        float castDist = data.stats.climbGrabDistance / Mathf.Max(Mathf.Pow(data.rb.rotation, 0.25f), 1.0f);
        RaycastHit2D climbHit = Physics2D.BoxCast(
            data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
            new Vector2(boxDepth, scanHeight),
            0,
            data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right,
            castDist - (boxDepth * 0.05f),
            Unit.CollisionMask
        );
        ExtDebug.DrawBox(
            data.rb.position + ((data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right) * castDist * 0.5f) - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
            new Vector2(castDist, scanHeight) * 0.5f,
            Quaternion.identity,
            Color.red
        );

        while (climbHit && scanHeight > (scanHeightInterval * 2))
        {
            scanHeight -= scanHeightInterval;
            RaycastHit2D scanHit = Physics2D.BoxCast(
                data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
                new Vector2(boxDepth, scanHeight),
                0,
                data.isFacingRight ? (Vector2)data.rb.transform.right : -(Vector2)data.rb.transform.right,
                castDist - (boxDepth * 0.05f),
                Unit.CollisionMask
            );
            if (scanHit && scanHit.distance <= climbHit.distance + minLedgeThickness)
            {
                
            } else {
                Debug.DrawLine(
                    data.rb.position - ((Vector2)data.rb.transform.up * (data.stats.standingHalfHeight - data.stats.maxClimbHeight + (scanHeight * 0.5f))),
                    climbHit.point,
                    Color.green,
                    data.stats.climbDuration
                );
                data.target = climbHit.point + (data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.climbGrabOffset.x + Vector2.up * data.stats.climbGrabOffset.y;
                return UnitState.LedgeGrab;
            }
            climbHit = scanHit;
        }
        return UnitState.Null;
    }

    private static UnitState TryDrop(UnitData data)
    {
        const float boxDepth = 0.1f;
        const float scanDepth = 0.5f;
        const float scanDepthInterval = 0.01f;
        float castDist = data.stats.standingScale.x;
        
        RaycastHit2D dropHit = Physics2D.BoxCast(
            data.rb.position,
            new Vector2(data.stats.standingScale.x, boxDepth),
            0,
            Vector2.down,
            data.stats.standingScale.y - (boxDepth * 0.05f),
            Unit.CollisionMask
        );
        //ExtDebug.DrawBox(
        //    data.rb.position + (Vector2.down * data.stats.standingScale.y * 0.5f),
        //    data.stats.standingScale * 0.5f,
        //    Quaternion.identity,
        //    dropHit ? Color.green : Color.red
        //);
        
        if(!dropHit) {
            float depth = 0.0f;
            while (depth <= scanDepth)
            {
                RaycastHit2D ledgeHit = Physics2D.Raycast(
                    data.rb.position + (Vector2.down * (data.stats.crawlingHalfHeight + depth)),
                    data.isFacingRight ? Vector2.left : Vector2.right,
                    data.stats.standingScale.x,
                    Unit.CollisionMask
                );
                Debug.DrawRay(
                    data.rb.position + (Vector2.down * (data.stats.crawlingHalfHeight + depth)),
                    (data.isFacingRight ? Vector2.left : Vector2.right) * (ledgeHit ? ledgeHit.distance : data.stats.standingScale.x),
                    ledgeHit ? Color.green : Color.red
                );
                if (ledgeHit)
                {
                    data.target = ledgeHit.point + (data.isFacingRight ? Vector2.left : Vector2.right) * data.stats.climbGrabOffset.x + Vector2.up * data.stats.climbGrabOffset.y;
                    data.isFacingRight = !data.isFacingRight;
                    data.animator.SetFacing(data.isFacingRight);
                    return UnitState.LedgeGrab;
                }
                depth += scanDepthInterval;
            }
        }

        return UnitState.Null;
    }
    
    private static bool CanCrawl(UnitData data)
    {
        const float sideCheckOffset = 0.1f;
        float heightOffset = -data.stats.standingHalfHeight + data.stats.crawlingHalfHeight + 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset), data.stats.crawlingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
        ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset), data.stats.crawlingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
        if (hit) {
            // Check left side
            hit = Physics2D.BoxCast(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + (-(Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
            ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + (-(Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
            if (hit) {
                // Check Right side
                hit = Physics2D.BoxCast(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + ((Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
                ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + ((Vector2)data.rb.transform.up * heightOffset) + ((Vector2)data.rb.transform.right * (data.stats.crawlingScale.x + sideCheckOffset - data.stats.standingScale.x) * 0.5f), data.stats.crawlingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
            }
        }
        return !hit;
    }

    private static bool CanStand(UnitData data, Vector2 offset)
    {
        RaycastHit2D hit = Physics2D.BoxCast(data.rb.position + offset, data.stats.standingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
        ExtDebug.DrawBox(new ExtDebug.Box(data.rb.position + offset, data.stats.standingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green);
        return !hit;
    }

    private static bool CanClimb(UnitData data) {
        Vector2 target = data.rb.position + (Vector2.up * (data.stats.standingHalfHeight + 0.1f - data.stats.climbGrabOffset.y)) + ((data.isFacingRight ? Vector2.left : Vector2.right) * data.stats.climbGrabOffset.x);
        RaycastHit2D hit = Physics2D.BoxCast(target, data.stats.standingScale, data.rb.rotation, Vector2.zero, 0, Unit.CollisionMask);
        ExtDebug.DrawBox(new ExtDebug.Box(target, data.stats.standingScale * 0.5f, Quaternion.Euler(0, 0, data.rb.rotation)), hit ? Color.red : Color.green, data.stats.climbDuration);
        return !hit;
    }
    
    private static bool FacingWall(UnitData data) {
        const float detectionDepth = 0.1f;
        float bodyWidth = data.isStanding ? data.stats.standingHalfWidth : data.stats.crawlingHalfWidth;
        RaycastHit2D wallHit = Physics2D.BoxCast(
            data.rb.position + (Vector2.up * data.stats.standingScale.y * 0.25f),
            new Vector2(detectionDepth, data.stats.standingScale.y * 0.5f),
            data.rb.rotation,
            data.isFacingRight ? data.rb.transform.right : -data.rb.transform.right,
            bodyWidth,
            Unit.CollisionMask
        );
        if (wallHit)
        {
            ExtDebug.DrawBoxCastOnHit(
                data.rb.position + (Vector2.up * data.stats.standingScale.y * 0.25f),
                new Vector2(detectionDepth, data.stats.standingScale.y * 0.5f) * 0.5f,
                Quaternion.Euler(0, 0, data.rb.rotation),
                data.isFacingRight ? data.rb.transform.right : -data.rb.transform.right,
                wallHit.distance,
                Color.green
            );
        }
        return wallHit;
    }
        
    #endregion
}
