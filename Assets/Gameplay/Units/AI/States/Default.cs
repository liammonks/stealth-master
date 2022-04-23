using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.States
{
    public class Default : BaseState
    {
        public Default(UnitData a_UnitData, AIData a_AIData) : base(a_UnitData, a_AIData) { }

        private AITask currentTask;
        private int patrolIndex = 0;

        public override AIState Initialise()
        {
            return AIState.Default;
        }

        public override AIState Execute()
        {
            // Stealth master spotted, stop tasks and change state
            Debug.Log(PlayerVisible());
            // If a task is executing, we wait
            if (currentTask != null)
            {
                return AIState.Default;
            }

            // Check hunger, interact with vending machines
            if (m_AIData.hunger >= 20.0f && LevelManager.FoodVendors.Length > 0)
            {
                // Move to nearest vendor
                Vector2 vendorDirection = NearestVendor() - m_UnitData.rb.position;
                Debug.DrawRay(m_UnitData.rb.position, vendorDirection, Color.red, AIController.tickInterval);
                m_UnitData.input.movement = Mathf.Clamp(vendorDirection.x, -1.0f, 1.0f);

                // Check for nearby vendor
                FoodVendor vendor = AvailableVendor();
                if (vendor != null)
                {
                    m_UnitData.input.movement = 0;
                    m_AIData.hunger = 0;
                }
                return AIState.Default;
            }

            // Complete tasks
            foreach (AITask task in m_AIData.tasks)
            {
                // Find available task
                if (task.IsAvailable())
                {
                    // Move to task
                    Vector2 taskDirection = (Vector2)task.transform.position - m_UnitData.rb.position;
                    Debug.DrawRay(m_UnitData.rb.position, taskDirection, Color.red, AIController.tickInterval);

                    // Execute task
                    if (taskDirection.magnitude <= task.interactionDistance)
                    {
                        m_UnitData.input.running = false;
                        m_UnitData.input.movement = 0;
                        currentTask = task;
                        currentTask.onTaskComplete += TaskComplete;
                        currentTask.Execute();
                    }
                    else // Move to task
                    {
                        m_UnitData.input.running = true;
                        m_UnitData.input.movement = Mathf.Clamp(taskDirection.x, -1.0f, 1.0f);
                    }
                    return AIState.Default;
                }
            }

            // Move between patrol points
            Vector2 patrolPoint = m_AIData.patrolPoints[patrolIndex];
            Vector2 moveDirection = patrolPoint - m_UnitData.rb.position;
            Debug.DrawRay(m_UnitData.rb.position, moveDirection, Color.red, AIController.tickInterval);
            m_UnitData.input.movement = Mathf.Clamp(moveDirection.x, -0.75f, 0.75f);
            if (moveDirection.magnitude <= 0.5f)
            {
                patrolIndex++;
                if (patrolIndex >= m_AIData.patrolPoints.Count) patrolIndex = 0;
            }

            return AIState.Default;
        }

        public override void End()
        {
            
        }

        private bool PlayerVisible()
        {
            Vector3 playerDirection = UnitHelper.Player.data.rb.position - m_UnitData.rb.position;

            LayerMask mask = LayerMask.GetMask(new string[] { "Environment", "Player" });
            RaycastHit2D hit = Physics2D.Linecast(m_UnitData.rb.position, UnitHelper.Player.data.rb.position, mask);
            if (hit.collider)
            {
                Debug.DrawLine(m_UnitData.rb.position, hit.point, Color.red);
            }
            return (hit.rigidbody == UnitHelper.Player.data.rb);
        }

        private Vector2 NearestVendor()
        {
            float nearestDistance = Mathf.Infinity;
            FoodVendor nearestVendor = null;
            foreach (FoodVendor vendor in LevelManager.FoodVendors)
            {
                float dist = ((Vector2)vendor.transform.position - m_UnitData.rb.position).sqrMagnitude;
                if (dist < nearestDistance)
                {
                    nearestDistance = dist;
                    nearestVendor = vendor;
                }
            }
            return nearestVendor.transform.position;
        }

        private FoodVendor AvailableVendor()
        {
            foreach (Interactable interactable in m_UnitData.interactables)
            {
                if (interactable is FoodVendor) return interactable as FoodVendor;
            }
            return null;
        }

        private void TaskComplete()
        {
            currentTask.onTaskComplete -= TaskComplete;
            currentTask = null;
        }
    }
}
