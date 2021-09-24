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

        // Execute Crawl
        if (data.input.crawling)
        {
            data.animator.Play("StandToCrawl");
            return UnitState.Crawl;
        }

        // Execute Run
        if (data.input.movement != 0 && Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed * 0.75f)
        {
            return UnitState.Run;
        }

        return UnitState.Idle;
    }
    
    private static UnitState RunState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Run");
            data.isStanding = true;
        }

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
        if(vaultState != UnitState.Null)
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

        if (data.input.crawling)
        {
            if ((data.possibleStates & UnitState.Slide) != 0 && Mathf.Abs(data.rb.velocity.x) > data.stats.walkSpeed)
            {
                // Execute Slide
                data.animator.Play("Slide");
                return UnitState.Slide;
            }
            else if ((data.possibleStates & UnitState.Crawl) != 0)
            {
                // Execute Crawl
                data.animator.Play("StandToCrawl");
                return UnitState.Crawl;
            }
        }
        
        // Return to Idle when below walk speed
        if (Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed * 0.5f)
        {
            return UnitState.Idle;
        }
        
        return UnitState.Run;
    }

    private static UnitState CrawlState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.isStanding = false;
        }
        
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

                // Execute Jump (only 100ms before returning idle)
                if (data.t < 0.1f && (data.possibleStates & UnitState.Jump) != 0 && data.input.jumpQueued)
                    return UnitState.Jump;

                if (data.t == 0.0f)
                    return UnitState.Idle;
            }
        }
        
        // Fall
        if(!data.isGrounded)
        {
            //return UnitState.Fall;
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

        if (data.isGrounded)
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
            }
        }
        else
        {
                data.ApplyDrag(data.stats.airDrag);
        }

        // Execute Crawl when speed drops below walking
        //if (Mathf.Abs(data.rb.velocity.x) < data.stats.walkSpeed)
        //{
        //    data.animator.Play("Crawl");
        //    return UnitState.Crawl;
        //}
        if (data.rb.velocity.magnitude < data.stats.walkSpeed)
        {
            data.animator.Play("Crawl");
            return UnitState.Crawl;
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
        }

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
                    if (data.t < 0.1f && (data.possibleStates & UnitState.Jump) != 0 && data.input.jumpQueued)
                    {
                        return UnitState.Jump;
                    }

                    if (data.t == 0.0f && (data.possibleStates & UnitState.Idle) != 0)
                    {
                        return UnitState.Idle;
                    }
                }
            }
            else
            {
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
                    data.animator.Play("Crawl");
                    return UnitState.Crawl;
                }
            }
        }
        else
        {
            data.ApplyDrag(data.stats.airDrag);
        }
        return UnitState.Dive;
    }

    private static UnitState JumpState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Jump");
            data.animator.Update(0);
            data.animator.Update(0);
            data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
            data.isStanding = true;
            data.groundSpringActive = false;
        }

        // Allow player to push towards movement speed while in the air
        Vector2 velocity = data.rb.velocity;
        if (Mathf.Abs(data.rb.velocity.x) < data.stats.runSpeed)
        {
            float desiredSpeed = (data.input.running ? data.stats.runSpeed : data.stats.walkSpeed) * data.input.movement;
            float deltaSpeedRequired = desiredSpeed - data.rb.velocity.x;
            velocity.x += deltaSpeedRequired * data.stats.airAcceleration;
        }
        
        
        if (data.isGrounded)
        {
            // As long as the jump is queued, apply jump force
            if (data.t > 0.0f)
            {
                velocity.y = data.stats.jumpForce;
                Vector2 horizontalVelocity = data.rb.velocity * Vector2.Dot(data.rb.velocity, data.rb.transform.right);
                data.rb.velocity = horizontalVelocity + ((Vector2)data.rb.transform.up * data.stats.jumpForce);
            }
            else
            {
                data.groundSpringActive = true;
                return UnitState.Idle;
            }
        }
        else
        {
            if (data.t == 0.0f)
            {
                data.groundSpringActive = true;
                return UnitState.Fall;
            }
        }

        data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
        data.rb.velocity = velocity;

        // Execute Dive
        if ((data.possibleStates & UnitState.Dive) != 0 && data.input.crawling)
        {
            data.groundSpringActive = true;
            return UnitState.Dive;
        }

        return UnitState.Jump;
    }

    private static UnitState FallState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.animator.Play("Fall");
            data.isStanding = true;
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
            return UnitState.Idle;
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
            data.target = data.target - data.rb.velocity;
            Debug.DrawRay(data.rb.position, data.target, Color.blue, 3);
        }
        if (data.t > 0.0f)
        {
            Vector2 velocity = data.rb.velocity;
            float springDisplacement = data.target.x;
            float springForce = springDisplacement * data.stats.springForce;
            float springDamping = data.rb.velocity.x * data.stats.springDamping;
            velocity.x += (springForce - springDamping) * Time.fixedDeltaTime;
            
            springDisplacement = data.target.y;
            springForce = springDisplacement * data.stats.springForce;
            springDamping = data.rb.velocity.y * data.stats.springDamping;
            velocity.y += (springForce - springDamping) * Time.fixedDeltaTime;

            data.rb.velocity = velocity;

            data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);
        }
        if(data.t == 0.0f)
        {
            data.groundSpringActive = true;
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
        return UnitState.Null;

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
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * (data.stats.vaultGrabDistance + data.stats.vaultMoveDistance)) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
                    Vector2.down,
                    data.stats.maxVaultHeight - data.stats.minVaultHeight
                );
                Debug.DrawRay(
                    data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * (data.stats.vaultGrabDistance + data.stats.vaultMoveDistance)) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
                    Vector2.down * (data.stats.maxVaultHeight - data.stats.minVaultHeight),
                    Color.red,
                    data.stats.vaultDuration
                );
                if(landingZoneHit)
                {
                    // Landing zone obstruction
                    Debug.DrawRay(
                        data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * (data.stats.vaultGrabDistance + data.stats.vaultMoveDistance)) + (Vector2.down * (data.stats.standingSpringDistance - data.stats.maxVaultHeight)),
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
                    data.target = data.rb.position + ((data.isFacingRight ? Vector2.right : Vector2.left) * data.stats.vaultMoveDistance) + (Vector2.up * data.stats.standingSpringDistance * 0.5f);
                    return UnitState.VaultOverState;
                }
            }
        }
        return UnitState.Null;
    }
    
    #endregion
}
