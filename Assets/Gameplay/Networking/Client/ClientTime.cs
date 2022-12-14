using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network.Shared;

namespace Network.Client
{

    public class ClientTime
    {

        private const float m_SecondsPerSync = 0.1f;

        private float m_SimulationTime => Time.fixedTime + m_SimulationTimeInitialOffset + m_SimulationTimeLatencyOffset;
        private float m_SimulationTimeInitialOffset;
        private float m_SimulationTimeLatencyOffset;
        private float m_Latency;

        private Client m_Client;

        public ClientTime(Client client)
        {
            m_Client = client;
        }

        public void InitialiseSimulationTime(float simulationTime)
        {
            m_SimulationTimeInitialOffset = simulationTime - Time.fixedTime;
            m_Client.StartCoroutine(SendSimulationTimeSync());
        }

        #region Time Sync

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
            m_Latency = rtt * 0.5f;
            // Simulation time difference without latency
            float difference = differnceWithLatency - m_Latency;

            m_SimulationTimeLatencyOffset += difference * 0.1f;
        }

        #endregion

    }

}