using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public enum TickOrder
{
    Spring,
    StateMachine,
    PhysicsObject_FindIntersections,
    PhysicsObject_HandleIntersections,
    Unit,
    InterpolateFollow,
    SMClient,
    SMServer,
    UnitInput
}

public interface ITick
{
    public TickOrder Order { get; }
    public void OnTick();
}

public class TickMachine : MonoBehaviour
{

    private float m_TimeUntilTick;

    private void Awake()
    {
        m_TimeUntilTick = Time.fixedDeltaTime;
    }

    private void Update()
    {
        m_TimeUntilTick -= Time.deltaTime;
        while (m_TimeUntilTick <= 0.0f)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);
            m_TimeUntilTick += Time.fixedDeltaTime;
        }
    }


}
