using DarkRift;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IRollback
{
    public List<StateData> GetSimulationState();
    public void SetSimulationState(IDarkRiftSerializable data);
}

public struct StateData
{
    public IRollback owner;
    public IDarkRiftSerializable data;

    public StateData(IRollback owner, IDarkRiftSerializable data)
    {
        this.owner = owner;
        this.data = data;
    }
}

public abstract class SimulationBehaviour : MonoBehaviour, IRollback
{

    protected virtual void Awake()
    {
        Simulation.RegisterSimulationBehaviour(this);
    }

    protected virtual void OnDestroy()
    {
        Simulation.UnregisterSimulationBehaviour(this);
    }

    public abstract void Simulate(float timeStep);

    public abstract List<StateData> GetSimulationState();

    public abstract void SetSimulationState(IDarkRiftSerializable data);

}
