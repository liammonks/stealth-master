using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Shared
{

    public class NetworkInputBuffer : MonoBehaviour
    {
        private INetworkTime m_NetworkTime;
        private NetworkUnitData m_UnitData;
        private NetworkPhysicsHistory m_PhysicsHistory;

        private Dictionary<float, List<BoolInputPacket>> m_BoolInputs = new Dictionary<float, List<BoolInputPacket>>();
        private Dictionary<float, List<FloatInputPacket>> m_FloatInputs = new Dictionary<float, List<FloatInputPacket>>();
        private List<float> m_UnprocessedTimes = new List<float>();
        private float m_SecondsPerProcess = 0.02f;

        private void Awake()
        {
            m_NetworkTime = NetworkManager.NetworkType == NetworkType.Server ?
                GetComponentInParent<Server.Server>().Time : GetComponentInParent<Client.Client>().Time;

            m_UnitData = GetComponent<NetworkUnitData>();
            m_PhysicsHistory = GetComponent<NetworkPhysicsHistory>();

            StartCoroutine(ProcessBuffer());
        }

        private IEnumerator ProcessBuffer()
        {
            if (m_UnprocessedTimes.Count == 0)
            {
                yield return new WaitForSeconds(m_SecondsPerProcess);
                StartCoroutine(ProcessBuffer());
                yield break;
            }

            // Find where to process from by getting oldest unprocessed input
            m_UnprocessedTimes.Sort();
            float processTime = m_UnprocessedTimes[0];

            if (processTime <= m_NetworkTime.SimulationTime)
            {
                m_PhysicsHistory.Rewind(processTime);

                do
                {
                    ProcessInputs(processTime);
                    m_PhysicsHistory.RecordState(processTime);
                    processTime += Time.fixedDeltaTime;
                    Physics2D.Simulate(Time.fixedDeltaTime);
                }
                while (processTime < m_NetworkTime.SimulationTime);
            }
            else
            {
                Debug.LogWarning("Client time is ahead of server");
            }

            yield return new WaitForSeconds(m_SecondsPerProcess);
            StartCoroutine(ProcessBuffer());
        }

        private void ProcessInputs(float time)
        {
            bool inputProcessed = false;

            if (m_BoolInputs.ContainsKey(time))
            {
                foreach (BoolInputPacket input in m_BoolInputs[time])
                {
                    switch (input.tag)
                    {
                        case BoolInputTag.Jumping:
                            m_UnitData.ClientUnits[input.clientID].Input.Jumping = input.value;
                            break;
                    }
                }
                inputProcessed = true;
            }

            if (inputProcessed)
            {
                m_UnprocessedTimes.Remove(time);
            }
        }

        private List<float> InputTimes()
        {
            List<float> inputTimes = new List<float>();
            inputTimes.AddRange(m_BoolInputs.Keys);
            inputTimes.AddRange(m_FloatInputs.Keys);
            inputTimes.Sort();
            inputTimes.Distinct();
            return inputTimes;
        }

        public void RegisterBoolInput(BoolInputPacket packet, bool processed = false)
        {
            if (m_BoolInputs.ContainsKey(packet.simulationTime))
            {
                m_BoolInputs[packet.simulationTime].Add(packet);
            }
            else
            {
                m_BoolInputs.Add(packet.simulationTime, new List<BoolInputPacket> { packet });
            }

            if (!processed && !m_UnprocessedTimes.Contains(packet.simulationTime))
            {
                m_UnprocessedTimes.Add(packet.simulationTime);
            }

            List<float> toRemove = new List<float>();
            foreach (KeyValuePair<float, List<BoolInputPacket>> kvp in m_BoolInputs)
            {
                float time = kvp.Key;
                if (m_NetworkTime.SimulationTime - time > NetworkPhysicsHistory.HistoryBufferTime)
                {
                    toRemove.Add(time);
                }
            }
            foreach (float timeToRemove in toRemove)
            {
                m_BoolInputs.Remove(timeToRemove);
            }
        }

        public void RegisterFloatInput(FloatInputPacket packet, bool processed = false)
        {
            if (m_FloatInputs.ContainsKey(packet.simulationTime))
            {
                m_FloatInputs[packet.simulationTime].Add(packet);
            }
            else
            {
                m_FloatInputs.Add(packet.simulationTime, new List<FloatInputPacket> { packet });
            }

            if (!processed && !m_UnprocessedTimes.Contains(packet.simulationTime))
            {
                m_UnprocessedTimes.Add(packet.simulationTime);
            }

            List<float> toRemove = new List<float>();
            foreach (KeyValuePair<float, List<FloatInputPacket>> kvp in m_FloatInputs)
            {
                float time = kvp.Key;
                if (m_NetworkTime.SimulationTime - time > NetworkPhysicsHistory.HistoryBufferTime)
                {
                    toRemove.Add(time);
                }
            }
            foreach (float timeToRemove in toRemove)
            {
                m_FloatInputs.Remove(timeToRemove);
            }
        }

    }

}