using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Simulation : MonoBehaviour
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
        m_SimulationBehaviours = m_SimulationBehaviours.OrderBy(x => x.Order).ToList();
    }

    #endregion

    #region Instance

    private float m_TimeUntilTick = TimeStep;

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
        foreach (SimulationBehaviour behaviour in m_SimulationBehaviours)
        {
            behaviour.Simulate(timeStep);
        }
        Physics2D.Simulate(timeStep);
    }

    #endregion

}
