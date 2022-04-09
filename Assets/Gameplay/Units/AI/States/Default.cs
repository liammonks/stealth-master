using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.States
{
    public class Default : BaseState
    {
        public Default(UnitData a_UnitData, AIData a_AIData) : base(a_UnitData, a_AIData) { }

        private AITask currentTask;

        public override AIState Initialise()
        {
            return AIState.Default;
        }

        public override AIState Execute()
        {
            if (currentTask != null)
            {
                return AIState.Default;
            }

            // Check hunger, interact with vending machines
            if (m_AIData.hunger >= 5.0f && LevelManager.FoodVendors.Length > 0)
            {
                // Move to nearest vendor
                Vector2 dir = NearestVendor() - m_UnitData.rb.position;
                Debug.DrawRay(m_UnitData.rb.position, dir, Color.red, AIController.tickInterval);
                m_UnitData.input.movement = Mathf.Clamp(dir.x, 0.0f, 1.0f);

                // Check for nearby vendor
                FoodVendor vendor = AvailableVendor();
                if (vendor != null)
                {
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

                    // Execute task
                        currentTask = task;
                        currentTask.onTaskComplete += TaskComplete;
                }
            }

            // Move between patrol points

            return AIState.Default;
        }

        public override void End()
        {
            
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
