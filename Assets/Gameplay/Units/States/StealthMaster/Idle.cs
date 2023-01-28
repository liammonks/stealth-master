using DarkRift;
using System.Collections.Generic;
using UnityEngine;

namespace States.StealthMaster
{

    public class Idle : States.Idle
    {
        public struct IdleData : IDarkRiftSerializable
        {
            public bool toCrawl;
            public float transitionDuration;

            public void Deserialize(DeserializeEvent e)
            {
                toCrawl = e.Reader.ReadBoolean();
                transitionDuration = e.Reader.ReadSingle();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(toCrawl);
                e.Writer.Write(transitionDuration);
            }
        }

        private IdleData m_Data = new IdleData();

        public Idle(Unit a_unit) : base(a_unit) { }

        public override UnitState Initialise()
        {
            m_Data.toCrawl = false;
            m_Data.transitionDuration = 0.0f;
            return base.Initialise();
        }
        
        public override UnitState Execute()
        {
            if (m_Data.toCrawl)
            {
                // Waiting to enter crawl state
                m_Data.transitionDuration = Mathf.Max(0.0f, m_Data.transitionDuration - Time.fixedDeltaTime);
                if (m_Data.transitionDuration == 0.0f) return UnitState.CrawlIdle;
                return UnitState.Idle;
            }

            UnitState state = base.Execute();
            if (state != UnitState.Idle) return state;

            // Push against wall
            if (unit.WallSpring.Intersecting)
            {
                if (unit.Animator.CurrentState != UnitAnimationState.AgainstWall)
                {
                    unit.Physics.Velocity = new Vector2(unit.FacingRight ? 0.5f : -0.5f, unit.Physics.Velocity.y);
                    unit.Animator.Play(UnitAnimationState.AgainstWall);
                }
            }
            else
            {
                unit.Animator.Play(UnitAnimationState.Idle);
            }
            // Execute Melee
            if (unit.Input.Melee)
            {
                return UnitState.Melee;
            }
            // Execute Crawl
            if (ToCrawl()) { return UnitState.CrawlIdle; }

            return UnitState.Idle;
        }

        private bool ToCrawl()
        {
            if (!unit.Input.Crawling) { return false; }
            if (unit.StateMachine.GetLastExecutionTime(UnitState.LedgeGrab) < 0.2f) { return false; }
            if (!unit.StateMachine.CanCrawl()) { return false; }

            // Play stand to crawl, wait before entering state
            unit.Animator.Play(UnitAnimationState.StandToCrawl);
            m_Data.transitionDuration = unit.Animator.CurrentStateLength;
            unit.SetBodyState(BodyState.Crawling, m_Data.transitionDuration);
            m_Data.toCrawl = true;
            return true;
        }

        #region Rollback

        public override List<StateData> GetSimulationState()
        {
            return new List<StateData> { new StateData(this, m_Data) };
        }

        public override void SetSimulationState(IDarkRiftSerializable data)
        {
            m_Data = (IdleData)data;
        }

        #endregion
    }

}