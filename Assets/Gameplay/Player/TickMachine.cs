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
    private const float TICK_RATE = 60;
    private const float TICK_INTERVAL = 1 / TICK_RATE;

    public static float DeltaTime => TICK_INTERVAL;
    public static uint TickCount { get; private set; }
    public static uint LastFrameTicks = 0;
    public static bool AutoTick = true;

    private float m_TimeUntilTick = TICK_INTERVAL;

    private void Update()
    {
        if (!AutoTick) { return; }
        m_TimeUntilTick -= Time.deltaTime;
        while (m_TimeUntilTick <= 0.0f)
        {
            Physics.Simulate(TICK_INTERVAL);
            m_TimeUntilTick += TICK_INTERVAL;
        }
    }

}
