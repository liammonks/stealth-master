using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    UnitInput,
    DeterministicTest
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

    private static Dictionary<TickOrder, UnityEvent> m_TickActions = new Dictionary<TickOrder, UnityEvent>();

    private float m_TimeUntilTick = TICK_INTERVAL;

    public static void Register(TickOrder order, UnityAction action)
    {
        if (!m_TickActions.ContainsKey(order))
        {
            m_TickActions.Add(order, new UnityEvent());
        }
        m_TickActions[order].AddListener(action);
    }

    public static void Unregister(TickOrder order, UnityAction action)
    {
        if (!m_TickActions.ContainsKey(order))
        {
            return;
        }
        m_TickActions[order].RemoveListener(action);
    }

    private void Update()
    {
        if (!AutoTick) { return; }
        m_TimeUntilTick -= Time.deltaTime;
        while (m_TimeUntilTick <= 0.0f)
        {
            Tick();
            m_TimeUntilTick += TICK_INTERVAL;
        }
    }

    public static void Tick()
    {
        TickCount++;
        foreach (TickOrder tickType in Enum.GetValues(typeof(TickOrder)))
        {
            if (!m_TickActions.ContainsKey(tickType)) { continue; }
            m_TickActions[tickType].Invoke();
        }
    }
}
