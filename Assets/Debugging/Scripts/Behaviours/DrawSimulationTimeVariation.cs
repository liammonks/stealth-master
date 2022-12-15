using Network.Client;
using System.Reflection;
using UnityEngine;

namespace Debugging.Behaviours
{

    public class DrawSimulationTimeVariation : DebugBehaviour<DrawSimulationTimeVariation>
    {
        [SerializeField]
        private Client m_Client;

        private const string m_Key = "SimulationTimeVariation";

        private float GetSimulationTimeVariation()
        {
            return (float)typeof(ClientTime).GetField("m_SimulationTimeLatencyOffset", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m_Client.Time);
        }

        private void OnDisable()
        {
            DebugGUI.RemovePersistent(m_Key);
        }

        private void Update()
        {
            DebugGUI.LogPersistent(m_Key, $"STV: {Mathf.RoundToInt(GetSimulationTimeVariation() * 1000)}ms");
        }

    }

}