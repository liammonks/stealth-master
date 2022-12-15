using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Shared
{

    public class NetworkInputBuffer : MonoBehaviour
    {
        private struct BoolInput
        {
            public ClientTag InputTag;
            public BoolInputPacket Packet;

            public BoolInput(ClientTag inputTag, BoolInputPacket packet)
            {
                InputTag = inputTag;
                Packet = packet;
            }
        }

        private struct FloatInput
        {
            public ClientTag InputTag;
            public FloatInputPacket Packet;

            public FloatInput(ClientTag inputTag, FloatInputPacket packet)
            {
                InputTag = inputTag;
                Packet = packet;
            }
        }

        private Dictionary<float, List<BoolInput>> m_BoolInput = new Dictionary<float, List<BoolInput>>();
        private Dictionary<float, List<FloatInput>> m_FloatInput = new Dictionary<float, List<FloatInput>>();
        private INetworkTime m_NetworkTime;

        private void Awake()
        {
            m_NetworkTime = NetworkManager.NetworkType == NetworkType.Server ?
                GetComponentInParent<Server.Server>().Time : GetComponentInParent<Client.Client>().Time;
        }

        public void RegisterBoolInput(ClientTag tag, BoolInputPacket packet)
        {
            if (m_BoolInput.ContainsKey(packet.simulationTime))
            {
                m_BoolInput[packet.simulationTime].Add(new BoolInput(tag, packet));
            }
            else
            {
                m_BoolInput.Add(packet.simulationTime, new List<BoolInput> { new BoolInput(tag, packet) });
            }

            List<float> toRemove = new List<float>();
            foreach (KeyValuePair<float, List<BoolInput>> kvp in m_BoolInput)
            {
                float time = kvp.Key;
                if (m_NetworkTime.SimulationTime - time > NetworkPhysicsHistory.HistoryBufferTime)
                {
                    toRemove.Add(time);
                }
            }
            foreach (float timeToRemove in toRemove)
            {
                m_BoolInput.Remove(timeToRemove);
            }
        }

        public void RegisterFloatInput(ClientTag tag, FloatInputPacket packet)
        {
            if (m_FloatInput.ContainsKey(packet.simulationTime))
            {
                m_FloatInput[packet.simulationTime].Add(new FloatInput(tag, packet));
            }
            else
            {
                m_FloatInput.Add(packet.simulationTime, new List<FloatInput> { new FloatInput(tag, packet) });
            }

            List<float> toRemove = new List<float>();
            foreach (KeyValuePair<float, List<FloatInput>> kvp in m_FloatInput)
            {
                float time = kvp.Key;
                if (m_NetworkTime.SimulationTime - time > NetworkPhysicsHistory.HistoryBufferTime)
                {
                    toRemove.Add(time);
                }
            }
            foreach (float timeToRemove in toRemove)
            {
                m_FloatInput.Remove(timeToRemove);
            }
        }

        public void ApplyInputs(float time)
        {
            
        }
    }

}