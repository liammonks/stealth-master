using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Simulation : SingletonBehaviour<Simulation>
{

    public static float Time => m_Time;
    private static float m_Time = 0;

    public static float TimeStep => 0.02f;

    private float m_TimeUntilTick = TimeStep;

    public float StateBufferDuration => m_StateBufferDuration;
    private const float m_StateBufferDuration = 1.0f;

    public Dictionary<float, List<StateData>> StateBuffer => m_StateBuffer;
    private Dictionary<float, List<StateData>> m_StateBuffer = new Dictionary<float, List<StateData>>();

    public Dictionary<float, List<StateData>> InputBuffer => m_InputBuffer;
    private Dictionary<float, List<StateData>> m_InputBuffer = new Dictionary<float, List<StateData>>();

    private List<SimulationBehaviour> m_SimulationBehaviours = new List<SimulationBehaviour>();
    private List<UnitInput> m_UnitInputs = new List<UnitInput>();

    public void RegisterUnitInput(UnitInput input)
    {
        if (m_UnitInputs.Contains(input))
        {
            Debug.LogError("Attempted to register the same UnitInput twice!", input);
            return;
        }
        m_UnitInputs.Add(input);
    }

    public void UnregisterUnitInput(UnitInput input)
    {
        if (!m_UnitInputs.Contains(input))
        {
            Debug.LogError("Attempted to unregister a UnitInput that is not registered!", input);
            return;
        }
        m_UnitInputs.Remove(input);
    }

    public void RegisterSimulationBehaviour(SimulationBehaviour behaviour)
    {
        if (m_SimulationBehaviours.Contains(behaviour))
        {
            Debug.LogError("Attempted to register the same SimulationBehaviour twice!", behaviour);
            return;
        }
        m_SimulationBehaviours.Add(behaviour);
    }

    public void UnregisterSimulationBehaviour(SimulationBehaviour behaviour)
    {
        if (!m_SimulationBehaviours.Contains(behaviour))
        {
            Debug.LogError("Attempted to unregister a SimulationBehaviour that is not registered!", behaviour);
            return;
        }
        m_SimulationBehaviours.Remove(behaviour);
    }

    public void OffsetTime(float offset)
    {
        m_Time += offset;
    }

    public void Rollback(float simulationTime)
    {
        if (!m_StateBuffer.ContainsKey(simulationTime))
        {
            Debug.LogError($"Rollback failed, {simulationTime} does not exist in the buffer");
            return;
        }

        m_Time = simulationTime;
        m_StateBuffer[simulationTime].ForEach(x => x.owner.SetSimulationState(x.data));
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
        // Clear state buffer data older than buffer duration
        List<float> oldBufferData = m_StateBuffer.Keys.Where(x => Time - x > m_StateBufferDuration).ToList();
        oldBufferData.ForEach(x => m_StateBuffer.Remove(x));

        if (m_StateBuffer.ContainsKey(Time))
        {
            // Current time has already been simulated, apply inputs

            m_StateBuffer[Time].Clear();
            m_InputBuffer[Time].Clear();
        }
        else
        {
            // Add new entry to state buffer for current time
            m_StateBuffer.Add(Time, new List<StateData>());
            m_InputBuffer.Add(Time, new List<StateData>());
        }

        Physics2D.Simulate(timeStep);

        // Simulate behaviours and collect state data
        foreach (SimulationBehaviour behaviour in m_SimulationBehaviours)
        {
            behaviour.Simulate(timeStep);
            m_StateBuffer[Time].AddRange(behaviour.GetSimulationState());
        }
        // Collect input data
        foreach (UnitInput unitInput in m_UnitInputs)
        {
            m_InputBuffer[Time].AddRange(unitInput.GetSimulationState());
        }

        m_Time = Mathf.Round((m_Time + timeStep) / timeStep) * timeStep;
    }

}
