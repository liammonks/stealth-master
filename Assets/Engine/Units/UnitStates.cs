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
        }
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
            if (!data.animator.GetCurrentAnimatorStateInfo(0).IsName("Run_Left") && !data.animator.GetCurrentAnimatorStateInfo(0).IsName("Run_Right"))
            {
                data.animator.Play("Run");
            }
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
            // Check Vault
            UnitState vaultState = TryVault(data);
            if (vaultState != UnitState.Null)
            {
                return vaultState;
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
            if (data.previousState == UnitState.Jump)
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
            data.animator.Play("Jump");
            data.animator.Update(0);
            data.animator.Update(0);
            data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
            data.isStanding = true;
            data.groundSpringActive = false;
            velocity.y = data.stats.jumpForce;
        }

        // Check Vault
        UnitState vaultState = TryVault(data);
        if (vaultState != UnitState.Null)
        {
            return vaultState;
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
            data.t = 0.5f;
            data.isStanding = true;
        }

        data.t -= Time.deltaTime;
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
            //Debug.DrawLine(data.rb.position, data.target, Color.blue, data.t);
            data.target = data.target - data.rb.position;
            Debug.DrawRay(data.rb.position, data.target, Color.blue, data.t);
        }

        if (data.t > 0.0f)
        {
            data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
            //data.rb.position = Vector2.Lerp(data.rb.position, data.target, Mathf.Pow(Mathf.Abs(data.t - data.stats.vaultDuration), 5));
            Debug.Log("PREV: " + data.rb.velocity);
            data.rb.velocity = (data.target / data.stats.vaultDuration);
            Debug.Log("CURR: " + data.rb.velocity);
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
            //Debug.DrawRay(data.target, Vector3.up , Color.blue, data.stats.vaultDuration);
        }
        // Get direction of the target position
        if (data.t > 0.0f)
        {
            // Apply movement towards target
            Debug.DrawLine(data.rb.position, data.target, Color.blue);
            //data.rb.velocity = data.target / data.stats.vaultDuration;
            data.t = Mathf.Max(0.0f, data.t - (Time.fixedDeltaTime / data.stats.vaultDuration));
        }
        if (data.t == 0.0f)
        {
            return UnitState.Idle;
        }
        return UnitState.VaultOnState;
    }
    
    #region Helpers
    
    private static UnitState TryVault(UnitData data)
    {
        RaycastHit2D nearHit = Physics2D.BoxCast(
            data.rb.position + (Vector2.down * data.stats.standingSpringDistance) + (Vector2.up * data.stats.maxVaultHeight * 0.5f),
            new Vector2(data.stats.vaultGrabDistance, data.stats.maxVaultHeight) * 0.8f,
            data.rb.rotation,
            data.isFacingRight ? Vector2.right : Vector2.left,
            data.stats.vaultGrabDistance * 0.5f,
            Unit.collisionMask
        );
        ExtDebug.DrawBoxCastOnHit(
            data.rb.position + (Vector2.down * data.stats.standingSpringDistance) + (Vector2.up * data.stats.maxVaultHeight * 0.5f),
            new Vector2(data.stats.vaultGrabDistance, data.stats.maxVaultHeight) * 0.8f * 0.5f,
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
            // Dont vault if ray is inside a collider
            if(vaultHit.distance > 0.0f)
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
                    return UnitState.Null;//UnitState.VaultOnState;
                }
                else
                {
                    // Landing zone clear
                    data.target = data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance);// + (Vector2.up * data.stats.standingSpringDistance * 0.5f);
                    return UnitState.VaultOverState;
                }
            }
        }
        return UnitState.Null;
    }
    
    #endregion
}
