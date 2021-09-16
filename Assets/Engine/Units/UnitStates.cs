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
        }

        // Execute Jump
        if ((data.possibleStates & UnitState.Jump) != 0 && data.ShouldJump())
        {
            return UnitState.Jump;
        }

        // Execute Crawl
        if ((data.possibleStates & UnitState.Crawl) != 0 && data.input.crawling)
        {
            data.animator.Play("StandToCrawl");
            data.collider.SetCrawling();
            return UnitState.Crawl;
        }

        // Execute Run if not moving towards collision
        if ((data.possibleStates & UnitState.Run) != 0)
        {
            if (data.input.movement > 0.0f && (data.collision & UnitCollision.Right) == 0)
            {
                return UnitState.Run;
            }
            if (data.input.movement < 0.0f && (data.collision & UnitCollision.Left) == 0)
            {
                return UnitState.Run;
            }
        }
        
        return UnitState.Idle;
    }
    
    private static UnitState RunState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Run");
        }
        // Apply movement input
        if (Mathf.Abs(data.velocity.x) < data.stats.runSpeed)
        {
            float speed = data.input.running ? data.stats.runSpeed : data.stats.walkSpeed;
            data.velocity.x += speed * data.input.movement * Time.fixedTime * data.stats.groundAuthority;
            data.velocity.x = Mathf.Clamp(data.velocity.x, -speed, speed);
        }

        // Execute Jump
        if ((data.possibleStates & UnitState.Jump) != 0 && data.ShouldJump())
        {
            return UnitState.Jump;
        }

        if (data.input.crawling)
        {
            if ((data.possibleStates & UnitState.Slide) != 0 && Mathf.Abs(data.velocity.x) > data.stats.walkSpeed)
            {
                // Execute Slide
                data.animator.Play("Slide");
                data.collider.SetCrawling();
                return UnitState.Slide;
            }
            else if ((data.possibleStates & UnitState.Crawl) != 0)
            {
                // Execute Crawl
                data.animator.Play("StandToCrawl");
                data.collider.SetCrawling();
                return UnitState.Crawl;
            }
        }
        
        if ((data.possibleStates & UnitState.Idle) != 0)
        {
            // Return to Idle when stopped
            if (data.velocity.magnitude == 0)
            {
                return UnitState.Idle;
            }
            // Return to Idle when moving towards collision
            if (data.velocity.x > 0.0f && (data.collision & UnitCollision.Right) != 0)
            {
                return UnitState.Idle;
            }
            if (data.velocity.x < 0.0f && (data.collision & UnitCollision.Left) != 0)
            {
                return UnitState.Idle;
            }
        }
        return UnitState.Run;
    }

    private static UnitState CrawlState(UnitData data, bool initialise)
    {
        // Apply movement input
        if (Mathf.Abs(data.velocity.x) < data.stats.walkSpeed)
        {
            data.velocity.x += data.stats.walkSpeed * data.input.movement * Time.fixedDeltaTime * data.stats.groundAuthority;
            data.velocity.x = Mathf.Clamp(data.velocity.x, -data.stats.walkSpeed, data.stats.walkSpeed);
        }

        // Return to Idle
        if ((data.possibleStates & UnitState.Idle) != 0 && !data.input.crawling && (data.collision & UnitCollision.Ground) != 0)
        {
            data.collider.SetStanding();
            return UnitState.Idle;
        }
        
        return UnitState.Crawl;
    }
    
    private static UnitState SlideState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            if (data.velocity.magnitude <= data.stats.runSpeed)
            {
                data.velocity *= data.stats.slideVelocityMultiplier;
            }
        }
        
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
                data.collider.SetStanding();
            }
            // Tick unit timer
            if (data.t != 0.0f)
            {
                data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);

                // Execute Jump
                if (data.t < 0.1f && (data.possibleStates & UnitState.Jump) != 0 && data.ShouldJump())
                {
                    return UnitState.Jump;
                }

                if (data.t == 0.0f && (data.possibleStates & UnitState.Idle) != 0)
                    return UnitState.Idle;
            }
        }

        // Execute Crawl when speed drops below walking
        if ((data.possibleStates & UnitState.Jump) != 0 && data.input.crawling && Mathf.Abs(data.velocity.x) <= data.stats.walkSpeed)
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
            data.velocity *= data.stats.diveVelocityMultiplier;
        }
        
        if ((data.collision & UnitCollision.Ground) != 0)
        {
            if (data.input.crawling == false || data.t != 0.0f)
            {

                // Set unit timer to exit animation duration
                if (data.t == 0)
                {
                    // Execute animation transition
                    data.animator.Play("DiveFlip");
                    // Update animator to transition to relevant state
                    data.animator.Update(0);
                    data.animator.Update(0);

                    data.t = data.animator.GetCurrentAnimatorStateInfo(0).length;
                    data.collider.SetStanding();
                }
                // Tick unit timer
                if (data.t != 0.0f)
                {
                    data.t = Mathf.Max(0.0f, data.t - Time.fixedDeltaTime);

                    // Execute Jump
                    if (data.t < 0.1f && (data.possibleStates & UnitState.Jump) != 0 && data.ShouldJump())
                    {
                        return UnitState.Jump;
                    }

                    if (data.t == 0.0f && (data.possibleStates & UnitState.Idle) != 0)
                        return UnitState.Idle;
                }
            }
            else
            {
                if (data.velocity.magnitude > data.stats.walkSpeed)
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
        return UnitState.Dive;
    }

    private static UnitState JumpState(UnitData data, bool initialise)
    {
        if(initialise)
        {
            data.animator.Play("Jump");
        }
        
        // Allow player to push towards movement speed while in the air
        float speed = data.input.running ? data.stats.runSpeed : data.stats.walkSpeed;
        if (Mathf.Abs(data.velocity.x) < speed)
        {
            data.velocity.x += speed * data.input.movement * Time.fixedDeltaTime * data.stats.airAuthority;
            data.velocity.x = Mathf.Clamp(data.velocity.x, -speed, speed);
        }

        // As long as the jump is queued, apply jump force
        if (data.input.jumpQueued)
        {
            data.velocity.y = data.stats.jumpForce;
        }

        // Execute Dive
        if ((data.possibleStates & UnitState.Dive) != 0 && data.input.crawling)
        {
            data.collider.SetCrawling();
            return UnitState.Dive;
        }

        // As soon as the player leaves the ground, the jump is no longer queued
        if ((data.collision & UnitCollision.Ground) == 0)
        {
            data.input.jumpQueued = false;
        }
        else
        {
            // If the player is touching the ground and the jump is no longer queued, unit landed
            if ((data.possibleStates & UnitState.Idle) != 0 && data.input.jumpQueued == false)
            {
                return UnitState.Idle;
            }
        }
        return UnitState.Jump;
    }
    
    private static UnitState VaultOverState(UnitData data, bool initialise)
    {
        float vaultDuration = 0.65f;
        if(initialise)
        {
            data.animator.Play("VaultOver");
            data.animator.Update(0);
            data.animator.Update(0);
            data.t = vaultDuration;
            data.target = data.target - data.position;
        }
        if (data.t > 0.0f)
        {
            // Apply movement
            data.velocity = data.target / (vaultDuration * 0.5f);
            data.t = Mathf.Max(0.0f, data.t - (Time.fixedDeltaTime / vaultDuration));
        }
        if(data.t == 0.0f)
        {
            return UnitState.Idle;
        }
        return UnitState.VaultOverState;
    }
    
    private static UnitState VaultOnState(UnitData data, bool initialise)
    {
        if (initialise)
        {
            data.animator.Play("VaultOn");
        }
        return UnitState.VaultOnState;
    }
    
}
