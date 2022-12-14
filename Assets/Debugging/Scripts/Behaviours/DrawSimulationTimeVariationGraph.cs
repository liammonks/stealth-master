using Network.Client;
using System.Reflection;
using UnityEngine;

namespace Debugging.Behaviours
{

    public class DrawSimulationTimeVariationGraph : DebugBehaviour<DrawSimulationTimeVariationGraph>
    {
        [SerializeField]
        private Client m_Client;

        private const string m_Key = "SimulationTimeVariationGraph";

        private float GetSimulationTimeVariation()
        {
            return (float)typeof(ClientTime).GetField("m_SimulationTimeLatencyOffset", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m_Client.Time);
        }

        private void OnEnable()
        {
            DebugGUI.SetGraphProperties(m_Key, "STV", -1, 1, 0, Color.white, false);
        }

        private void OnDisable()
        {
            DebugGUI.RemoveGraph(m_Key);
        }

        private void Update()
        {
            DebugGUI.Graph(m_Key, GetSimulationTimeVariation());
        }

    }

}