using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AI
{
    using States;
    
    public enum AIState
    {
        Null,
        Default,
        Alert,
        Pursuit
    }

    [System.Serializable]
    public class AIData
    {
        public List<AITask> tasks = new List<AITask>();
        public List<Vector2> patrolPoints = new List<Vector2>();
        public float hunger = 0.0f;
        public float fear = 0.0f;
    }

    [RequireComponent(typeof(Unit))]
    public class AIController : MonoBehaviour
    {
        public static float tickInterval = 0.1f;
        
        public Dictionary<AIState, BaseState> states = new Dictionary<AIState, BaseState>();
        public AIData data = new AIData();

        private AIState currentState = AIState.Default;
        private AIState previousState = AIState.Null;

        private void Start()
        {
            StartCoroutine(Tick());
        }

        private IEnumerator Tick()
        {
            // Execute State
            while (currentState != previousState)
            {
                previousState = currentState;
                currentState = states[currentState].Initialise();
            }
            currentState = states[currentState].Execute();

            // Update Data
            data.hunger += tickInterval;

            yield return new WaitForSecondsRealtime(tickInterval);
            StartCoroutine(Tick());
        }

    }
}