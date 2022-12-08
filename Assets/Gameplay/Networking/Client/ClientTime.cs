using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network.Shared;

namespace Network.Client
{

    public class ClientTime
    {
        public double SimulationTime => m_SimulationTime;

        private double m_SimulationTime => Time.fixedTimeAsDouble + m_SimulationTimeOffset;
        private double m_SimulationTimeOffset;
        private double m_Latency;

        private Client m_Client;

        public ClientTime(Client client)
        {
            m_Client = client;
        }

        public void InitialiseSimulationTime(double simulationTime)
        {
            m_SimulationTimeOffset = simulationTime - Time.fixedTimeAsDouble;
            m_Client.StartCoroutine(SendSimulationTimeSync(0));
        }

        public void ReceivedSimulationTimeSync(SimulationTimeSync simulationTimeSync)
        {
            double T2 = SimulationTime;
            // Server to client time difference + server to client latency
            double differnceWithLatency = simulationTimeSync.T1 - simulationTimeSync.T0;
            // Round trip time
            double rtt = T2 - simulationTimeSync.T0;
            // Approximate latency as half RTT
            m_Latency = rtt / 2;
            // Server to client time difference without latency
            double difference = differnceWithLatency - m_Latency;

            m_SimulationTimeOffset += difference * 0.5f;
            m_Client.StartCoroutine(SendSimulationTimeSync(0));
        }

        private IEnumerator SendSimulationTimeSync(float delay)
        {
            yield return new WaitForSeconds(delay);

            SimulationTimeSync simulationTimeSync = new SimulationTimeSync();
            simulationTimeSync.T0 = m_Client.Time.SimulationTime;
            m_Client.MessageSender.SendMessage(ClientTag.SimulationTimeRequest, simulationTimeSync);
        }
    }

}