using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network.Shared;

namespace Network.Client
{

    public class ClientTime : INetworkTime
    {

        public float SimulationTime => m_SimulationTime;

        private const float m_SecondsPerSync = 0.5f;
        private const int m_DifferenceHistoryCount = 5;

        private float m_SimulationTime => Time.fixedTime + m_SimulationTimeInitialOffset + m_SimulationTimeLatencyOffset;

        private float m_SimulationTimeInitialOffset;
        private float m_SimulationTimeLatencyOffset;
        private float m_SimulationLatency;

        private Client m_Client;

        private List<float> m_SimulationTimeDifferenceHistory = new List<float>();

        public ClientTime(Client client)
        {
            m_Client = client;
        }

        public void InitialiseSimulationTime(float simulationTime)
        {
            m_SimulationTimeInitialOffset = simulationTime - Time.fixedTime;
            m_Client.StartCoroutine(SendSimulationTimeSync());
        }

        private IEnumerator SendSimulationTimeSync()
        {
            SimulationTimeSync simulationTimeSync = new SimulationTimeSync();
            simulationTimeSync.T0 = m_SimulationTime;
            m_Client.MessageSender.SendMessage(ClientTag.SimulationTimeRequest, simulationTimeSync);

            yield return new WaitForSeconds(m_SecondsPerSync);
            m_Client.StartCoroutine(SendSimulationTimeSync());
        }

        public void ReceivedSimulationTimeSync(SimulationTimeSync simulationTimeSync)
        {
            float T2 = m_SimulationTime;
            // simulation time difference + client to server latency
            float differnceWithLatency = simulationTimeSync.T1 - simulationTimeSync.T0;
            // Round trip time
            float rtt = T2 - simulationTimeSync.T0;
            // Approximate latency as half RTT
            m_SimulationLatency = rtt * 0.5f;
            // Simulation time difference without latency
            float difference = RoundToFixedTimeStep(differnceWithLatency - m_SimulationLatency);

            m_SimulationTimeDifferenceHistory.Add(difference);
            if (m_SimulationTimeDifferenceHistory.Count > m_DifferenceHistoryCount)
            {
                m_SimulationTimeDifferenceHistory.RemoveAt(0);
            }

            float varianceMultiplier = RoundToFixedTimeStep(difference * Mathf.Clamp(SimulationTimeVariance(), 0.1f, 1.0f));
            m_SimulationTimeLatencyOffset += varianceMultiplier;
        }

        private float RoundToFixedTimeStep(float toRound)
        {
            return Mathf.Round(toRound / Time.fixedDeltaTime) * Time.fixedDeltaTime;
        }

        private float SimulationTimeVariance()
        {
            float variance = 0;
            foreach (float difference in m_SimulationTimeDifferenceHistory)
            {
                variance += Mathf.Abs(difference);
            }
            return variance / m_SimulationTimeDifferenceHistory.Count;
        }
    }

}