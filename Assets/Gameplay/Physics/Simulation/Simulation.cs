using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.WSA;

public class Simulation : SingletonBehaviour<Simulation>
{
    #region Static
    
    public static float Time => m_Time;
    public static float TimeStep => 0.02f;

    private static float m_Time = 0;
    private static List<SimulationBehaviour> m_SimulationBehaviours = new List<SimulationBehaviour>();

    public static void OffsetTime(float offset)
    {
        m_Time += offset;
    }

    public static void RegisterSimulationBehaviour(SimulationBehaviour behaviour)
    {
        if (m_SimulationBehaviours.Contains(behaviour))
        {
            Debug.LogError("Attempted to register the same SimulationBehaviour twice!", behaviour);
            return;
        }
        m_SimulationBehaviours.Add(behaviour);
    }

    public static void UnregisterSimulationBehaviour(SimulationBehaviour behaviour)
    {
        if (!m_SimulationBehaviours.Contains(behaviour))
        {
            Debug.LogError("Attempted to unregister a SimulationBehaviour that is not registered!", behaviour);
            return;
        }
        m_SimulationBehaviours.Remove(behaviour);
    }

    #endregion

    #region Instance

    private float m_TimeUntilTick = TimeStep;

    // Rollback
    public float StateBufferDuration => m_StateBufferDuration;
    public Dictionary<float, List<StateData>> StateBuffer => m_StateBuffer;

    private const float m_StateBufferDuration = 1.0f;
    private Dictionary<float, List<StateData>> m_StateBuffer = new Dictionary<float, List<StateData>>();

    public void Rollback(float simulationTime)
    {
        if (!m_StateBuffer.ContainsKey(simulationTime))
        {
            Debug.LogError($"Rollback failed, {simulationTime} does not exist in the buffer");
            return;
        }

        m_Time = simulationTime;
        m_StateBuffer[simulationTime].ForEach(x => x.owner.SetSimulationState(x.data));
        Physics2D.SyncTransforms();
    }

    private void Update()
    {
        m_TimeUntilTick -= UnityEngine.Time.deltaTime;

        while (m_TimeUntilTick <= 0.0f)
        {
            Simulate(TimeStep);
            m_TimeUntilTick += TimeStep;
        }
    }

    private void Simulate(float timeStep)
    {
        // Clear old state buffer data
        List<float> oldBufferData = m_StateBuffer.Keys.Where(x => Time - x > m_StateBufferDuration).ToList();
        oldBufferData.ForEach(x => m_StateBuffer.Remove(x));

        // Add new entry to state buffer for current time
        if (m_StateBuffer.ContainsKey(Time))
        {
            m_StateBuffer[Time].Clear();
        }
        else
        {
            m_StateBuffer.Add(Time, new List<StateData>());
        }

        Physics2D.Simulate(timeStep);

        // Collect simulation state data
        foreach (SimulationBehaviour behaviour in m_SimulationBehaviours)
        {
            behaviour.Simulate(timeStep);
            m_StateBuffer[Time].AddRange(behaviour.GetSimulationState());
        }

        m_Time += timeStep;
    }

    #endregion

}
