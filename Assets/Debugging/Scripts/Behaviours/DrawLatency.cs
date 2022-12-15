using Network.Client;
using System.Reflection;
using UnityEngine;

namespace Debugging.Behaviours
{

    public class DrawLatency : DebugBehaviour<DrawLatency>
    {
        [SerializeField]
        private Client m_Client;

        private const string m_Key = "LatencyLog";

        private float GetLatency()
        {
            return (float)typeof(ClientTime).GetField("m_SimulationLatency", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m_Client.Time);
        }

        private void OnDisable()
        {
            DebugGUI.RemovePersistent(m_Key);
        }

        private void Update()
        {
            DebugGUI.LogPersistent(m_Key, $"Latency: {Mathf.RoundToInt(GetLatency() * 1000)}ms");
        }

    }

}