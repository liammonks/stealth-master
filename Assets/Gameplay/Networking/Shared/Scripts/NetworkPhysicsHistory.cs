using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Shared
{

    public class NetworkPhysicsHistory : MonoBehaviour
    {
        private struct RigidbodyData
        {
            public Vector3 Position;
            public Vector3 Velocity;

            public RigidbodyData(Vector3 position, Vector3 velocity)
            {
                Position = position;
                Velocity = velocity;
            }
        }

        private struct HistoryEntry
        {
            public Dictionary<Rigidbody2D, RigidbodyData> RigidbodyData;

            public HistoryEntry(Dictionary<Rigidbody2D, RigidbodyData> rigidbodyData)
            {
                RigidbodyData = rigidbodyData;
            }
        }


        public static readonly float HistoryBufferTime = 1.0f;

        private INetworkTime m_NetworkTime;

        private List<Rigidbody2D> m_Rigidbodies;
        private Dictionary<float, HistoryEntry> m_History = new Dictionary<float, HistoryEntry>();

        private void Awake()
        {
            m_NetworkTime = NetworkManager.NetworkType == NetworkType.Server ?
                GetComponentInParent<Server.Server>().Time : GetComponentInParent<Client.Client>().Time;

            m_Rigidbodies = FindObjectsOfType<Rigidbody2D>().ToList();
            GetComponent<NetworkUnitData>().OnUnitSpawned += OnUnitSpawned;
            GetComponent<NetworkUnitData>().OnUnitDestroyed += OnUnitDestroyed;
        }

        public void Rewind(float time)
        {
            time = RoundToFixedTimeStep(time);
            if (!m_History.ContainsKey(time))
            {
                Debug.LogError($"Failed to rewind simulation to {time},\nTime was not stored in history, current time is {m_NetworkTime.SimulationTime}");
                return;
                //Debug.LogError($"-- Available times in history --");
                //foreach (float historyTime in m_History.Keys)
                //{
                //    Debug.LogError(historyTime);
                //}
            }

            // Revert rigidbodies to point in time
            HistoryEntry entry = m_History[time];
            foreach (KeyValuePair<Rigidbody2D, RigidbodyData> rigidbodyData in entry.RigidbodyData)
            {
                Rigidbody2D rb = rigidbodyData.Key;
                RigidbodyData data = rigidbodyData.Value;

                rb.position = data.Position;
                rb.velocity = data.Velocity;
            }
        }

        public void RecordState(float simulationTime)
        {
            // Collect data
            Dictionary<Rigidbody2D, RigidbodyData> newRigidbodyData = new Dictionary<Rigidbody2D, RigidbodyData>();
            foreach (Rigidbody2D rb in m_Rigidbodies)
            {
                RigidbodyData data = new RigidbodyData(rb.position, rb.velocity);
                newRigidbodyData.Add(rb, data);
            }

            if (m_History.ContainsKey(RoundToFixedTimeStep(simulationTime)))
            {
                m_History[RoundToFixedTimeStep(simulationTime)] = new HistoryEntry(newRigidbodyData);
            }
            else
            {
                m_History.Add(RoundToFixedTimeStep(simulationTime), new HistoryEntry(newRigidbodyData));
            }
        }

        private void OnUnitSpawned(Unit unit)
        {
            m_Rigidbodies.Add(unit.Physics.Rigidbody);
        }

        private void OnUnitDestroyed(Unit unit)
        {
            m_Rigidbodies.Remove(unit.Physics.Rigidbody);
        }

        private void FixedUpdate()
        {
            // Record new data
            RecordState(m_NetworkTime.SimulationTime);

            // Clear old data
            List<float> toRemove = new List<float>();
            foreach (float historyEntryTime in m_History.Keys)
            {
                if (m_NetworkTime.SimulationTime - historyEntryTime > HistoryBufferTime)
                {
                    toRemove.Add(historyEntryTime);
                }
            }
            foreach (float historyToRemove in toRemove)
            {
                m_History.Remove(historyToRemove);
            }

            // Debug data
            foreach (KeyValuePair<float, HistoryEntry> historyEntry in m_History)
            {
                float entryTime = historyEntry.Key;
                HistoryEntry entry = historyEntry.Value;
                float alpha = 1.01f - ((m_NetworkTime.SimulationTime - entryTime) / 0.5f);

                foreach (KeyValuePair<Rigidbody2D, RigidbodyData> oldRigidbodyData in entry.RigidbodyData)
                {
                    RigidbodyData data = oldRigidbodyData.Value;

                    DebugExtension.DebugPoint(data.Position, Color.red * alpha, 0.1f);
                    Debug.DrawRay(data.Position, data.Velocity, Color.green * alpha);
                }
            }
        }

        private float RoundToFixedTimeStep(float toRound)
        {
            return Mathf.Round(toRound / Time.fixedDeltaTime) * Time.fixedDeltaTime;
        }

    }

}