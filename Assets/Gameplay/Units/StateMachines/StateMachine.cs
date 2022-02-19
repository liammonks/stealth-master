using System;
using System.Collections.Generic;
using UnityEngine;
using States;

[RequireComponent(typeof(Unit))]
public abstract class StateMachine : MonoBehaviour
{
    public UnitState State => currentState;

    public delegate void OnStateUpdated(UnitState state);
    public event OnStateUpdated onStateUpdated;

    protected Unit unit;
    protected UnitData data;
    protected UnitState currentState = UnitState.Idle;
    protected UnitState previousState = UnitState.Null;
    protected Dictionary<UnitState, BaseState> states = new Dictionary<UnitState, BaseState>();

    private BaseState overrideState;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
        data = unit.data;
    }
    
    public void Reset()
    {
        currentState = UnitState.Idle;
        previousState = UnitState.Null;
        
        data.target = Vector2.zero;
        data.previousState = UnitState.Null;
        data.t = 0.0f;
        data.stateDuration = 0.0f;
        data.groundSpringActive = true;
        data.isSlipping = false;
        data.isStanding = true;
        data.rb.isKinematic = false;
        data.rb.gravityScale = 1.0f;
        data.rb.simulated = true;
    }
    
    private void FixedUpdate()
    {
        if (overrideState != null)
        {
            currentState = overrideState.Execute();
            if (currentState == UnitState.Null) return;
            previousState = currentState;
            currentState = states[currentState].Initialise();
        }
        
        while (currentState != previousState)
        {
            data.previousState = previousState;
            previousState = currentState;
            currentState = states[currentState].Initialise();
            data.stateDuration = 0.0f;
            onStateUpdated.Invoke(currentState);
        }
        data.stateDuration += Time.fixedDeltaTime;
        currentState = states[currentState].Execute();
        Vector2 pos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up);
        //Log.Text("STATE", currentState.ToString(), pos, Color.green, Time.fixedDeltaTime);
    }
    
    public void SetState(UnitState state)
    {
        if (!states.ContainsKey(state)) return;
        overrideState = null;
        currentState = state;
    }
    
    public void SetState(BaseState state)
    {
        overrideState = state;
        previousState = UnitState.Null;
        if(overrideState == null) return;
        overrideState.Initialise();
    }
}
