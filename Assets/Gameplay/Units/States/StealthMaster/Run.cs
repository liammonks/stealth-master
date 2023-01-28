using DarkRift;
using System.Collections.Generic;
using UnityEngine;

namespace States.StealthMaster
{
    public class Run : States.Run
    {
        public struct RunData : IDarkRiftSerializable
        {
            public bool toCrawl;
            public float transitionDuration;

            public void Deserialize(DeserializeEvent e)
            {
                e.Reader.ReadBoolean();
                e.Reader.ReadSingle();
            }

            public void Serialize(SerializeEvent e)
            {
                e.Writer.Write(toCrawl);
                e.Writer.Write(transitionDuration);
            }
        }

        private RunData m_Data = new RunData();

        public Run(Unit a_unit) : base(a_unit) { }
        
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
                m_Data.transitionDuration = Mathf.Max(0.0f, m_Data.transitionDuration - Time.fixedDeltaTime);
                if (m_Data.transitionDuration == 0.0f) return UnitState.Crawl;
                return UnitState.Run;
            }
            
            UnitState state = base.Execute();
            if (state != UnitState.Run) return state;
            Vector2 velocity = unit.Physics.Velocity;

            // Vault / Climb
            if (unit.StateMachine.TryVaultOver())
            {
                return UnitState.VaultOver;
            }
            if (unit.StateMachine.TryVaultOn())
            {
                return UnitState.VaultOn;
            }
            if (unit.StateMachine.TryLedgeGrab())
            {
                return UnitState.LedgeGrab;
            }

            // Execute Melee
            if (unit.Input.Melee)
            {
                return UnitState.Melee;
            }

            // Crawl / Slide
            if (unit.Input.Crawling && unit.StateMachine.CanCrawl())
            {
                if (Mathf.Abs(velocity.x) > unit.Settings.walkSpeed)
                {
                    // Execute Slide
                    unit.Animator.Play(UnitAnimationState.Slide);
                    return UnitState.Slide;
                }
                else
                {
                    // Play stand to crawl, wait before entering state
                    unit.Animator.Play(UnitAnimationState.StandToCrawl);
                    m_Data.transitionDuration = unit.Animator.CurrentStateLength;
                    unit.SetBodyState(BodyState.Crawling, m_Data.transitionDuration);
                    m_Data.toCrawl = true;
                }
            }

            return UnitState.Run;
        }

        public override List<StateData> GetSimulationState()
        {
            List<StateData> data = base.GetSimulationState();
            data.Add(new StateData(this, m_Data));
            return data;
        }

        public override void SetSimulationState(IDarkRiftSerializable data)
        {
            base.SetSimulationState(data);
            m_Data = (RunData)data;
        }
    }
}