using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimulationBehaviour : MonoBehaviour
{
    public enum SimulationOrder
    {
        Default,
        Rollback
    }

    public SimulationOrder Order => m_Order;
    protected SimulationOrder m_Order = SimulationOrder.Default;

    private void Awake()
    {
        OnAwake();
        Simulation.RegisterSimulationBehaviour(this);
    }

    protected virtual void OnAwake() { }

    public abstract void Simulate(float timeStep);

}
