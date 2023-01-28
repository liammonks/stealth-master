using DarkRift;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace States
{
    public class Jump : BaseState
    {
        private struct JumpData : IDarkRiftSerializable
        {
            public float jumpDuration;

            public void Deserialize(DeserializeEvent e)
            {
                jumpDuration = e.Reader.ReadSingle();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(jumpDuration);
            }
        }

        private JumpData m_Data = new JumpData();

        public Jump(Unit a_unit) : base(a_unit) { }

        public override UnitState Initialise()
        {
            unit.Animator.Play(UnitAnimationState.Jump);
            m_Data.jumpDuration = unit.Animator.CurrentStateLength;
            unit.GroundSpring.enabled = false;

            Vector2 velocity = unit.Physics.Velocity;
            velocity.y = unit.StateMachine.PreviousState == UnitState.LedgeGrab ? unit.Settings.wallJumpForce.y : unit.Settings.jumpForce;

            if (unit.GroundSpring.AttachedPhysics)
            {
                velocity += unit.GroundSpring.AttachedPhysics.velocity;
                unit.GroundSpring.AttachedPhysics = null;
            }

            unit.Physics.Velocity = velocity;
            unit.Physics.SkipDrag();

            return UnitState.Jump;
        }
        
        public override UnitState Execute()
        {
            m_Data.jumpDuration = Mathf.Max(0.0f, m_Data.jumpDuration - Time.fixedDeltaTime);

            //Allow player to push towards movement speed while in the air
            if (unit.Input.Movement != 0)
            {
                Vector2 velocity = unit.Physics.Velocity;
                if (Mathf.Abs(velocity.x) < unit.Settings.runSpeed)
                {
                    float desiredSpeed = (unit.Input.Running ? unit.Settings.runSpeed : unit.Settings.walkSpeed) * unit.Input.Movement;
                    float deltaSpeedRequired = desiredSpeed - velocity.x;
                    velocity.x += deltaSpeedRequired * unit.Settings.airAcceleration * Time.fixedDeltaTime;
                    unit.Physics.Velocity = velocity;
                }
            }

            // End of jump animation
            if (m_Data.jumpDuration == 0.0f)
            {
                unit.GroundSpring.enabled = true;
                return unit.GroundSpring.Intersecting ? UnitState.Idle : UnitState.Fall;
            }

            return UnitState.Jump;
        }

        public override void Deinitialise()
        {

        }

        #region Rollback

        public override List<StateData> GetSimulationState()
        {
            return new List<StateData> { new StateData(this, m_Data) };
        }

        public override void SetSimulationState(IDarkRiftSerializable data)
        {
            m_Data = (JumpData)data;
        }

        #endregion
    }
}