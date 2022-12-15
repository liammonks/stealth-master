using Network.Client;
using System.Reflection;
using UnityEngine;

namespace Debugging.Behaviours
{

    public class DrawSimulationTime : DebugBehaviour<DrawSimulationTime>
    {
        [SerializeField]
        private Client m_Client;

        private const string m_Key = "SimulationTime";

        private float GetSimulationTime()
        {
            return (float)typeof(ClientTime).GetProperty("m_SimulationTime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m_Client.Time);
        }

        private void OnDisable()
        {
            DebugGUI.RemovePersistent(m_Key);
        }

        private void Update()
        {
            DebugGUI.LogPersistent(m_Key, $"Simulation Time: {GetSimulationTime()}s");
        }

    }

}