using Network.Client;
using System.Reflection;
using UnityEngine;

namespace Debugging.Behaviours
{

    public class DrawLatencyGraph : DebugBehaviour<DrawLatencyGraph>
    {
        [SerializeField]
        private Client m_Client;

        private const string m_Key = "LatencyGraph";

        private float GetLatency()
        {
            return (float)typeof(ClientTime).GetField("m_SimulationLatency", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m_Client.Time);
        }

        private void OnEnable()
        {
            DebugGUI.SetGraphProperties(m_Key, "Latency", 0, 0, 0, Color.yellow, true);
        }

        private void OnDisable()
        {
            DebugGUI.RemoveGraph(m_Key);
        }

        private void Update()
        {
            DebugGUI.Graph(m_Key, Mathf.RoundToInt(GetLatency() * 1000));
        }

    }

}