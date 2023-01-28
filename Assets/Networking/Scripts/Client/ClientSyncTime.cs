using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSyncTime : MonoBehaviour
{
    private const float m_MinSyncInterval = 0.1f;

    private bool m_AwaitingResponse = false;

    private const float m_LatencyHistorySize = 3;
    private List<float> m_LatencyHistory = new List<float>();

    private const float m_TimeDifferenceVarianceMultiplier = 1;
    private const float m_TimeDifferenceHistorySize = 3;
    private List<float> m_TimeDifferenceHistory = new List<float>();

    private void OnEnable()
    {
        if (!m_AwaitingResponse)
        {
            StartCoroutine(SendSync(Simulation.Time));
        }
    }

    private IEnumerator SendSync(float lastSyncTime)
    {
        float nextSyncTime = lastSyncTime + m_MinSyncInterval;
        yield return new WaitWhile(() => Simulation.Time < nextSyncTime);

        SyncTime packet = new SyncTime();
        packet.clientRequestTime = Simulation.Time;
        ClientMessageSender.SendMessage(Tag.SyncTime, packet);
        m_AwaitingResponse = true;
    }

    public void OnSyncTimeReceived(SyncTime packet)
    {
        m_AwaitingResponse = false;

        float currentTime = Simulation.Time;
        float rtt = currentTime - packet.clientRequestTime;
        float latency = rtt * 0.5f;
        float timeDifference = (packet.clientRequestTime - packet.serverReceiveTime) + latency;

        // Calculate and send latency
        m_LatencyHistory.Add(latency);
        if (m_LatencyHistory.Count > m_LatencyHistorySize)
        {
            m_LatencyHistory.RemoveAt(0);
        }
        LatencyUpdate latencyPacket = new LatencyUpdate();
        latencyPacket.clientID = NetworkManager.Client.ID;
        latencyPacket.latency = LatencyMean();
        ClientMessageSender.SendMessage(Tag.LatencyUpdate, latencyPacket);

        // Calculate time difference variation
        m_TimeDifferenceHistory.Add(timeDifference);
        if (m_TimeDifferenceHistory.Count > m_TimeDifferenceHistorySize)
        {
            m_TimeDifferenceHistory.RemoveAt(0);
        }
        float timeDifferenceVariance = TimeDifferenceVariance();

        // Offset time, reduce offset multiplier the more accurate our variance is
        float timeDifferenceMultiplier = Mathf.Clamp(timeDifferenceVariance * m_TimeDifferenceVarianceMultiplier, 0, 1);
        Simulation.OffsetTime(-timeDifference * timeDifferenceMultiplier);

        if (true)
        {
            Debug.Log("-- Received Time Sync --");
            Debug.Log($"-- Client Request Time: {packet.clientRequestTime}");
            Debug.Log($"-- Server Receive Time: {packet.serverReceiveTime}");
            Debug.Log($"-- Client Current Time: {currentTime}");
            Debug.Log($"-- Client Altered Time: {Simulation.Time}");
            Debug.Log($"-- Latency: {latency}");
            Debug.Log($"-- Time Difference: {timeDifference}");
            Debug.Log($"-- Time Difference Variance: {timeDifferenceVariance}");
            Debug.Log($"-- Time Difference Multiplier: {timeDifferenceMultiplier}");
            Debug.Log("------------------------");
        }

        if (isActiveAndEnabled)
        {
            StartCoroutine(SendSync(packet.clientRequestTime));
        }
    }

    private float LatencyMean()
    {
        float mean = 0;
        foreach (float latency in m_LatencyHistory)
        {
            mean += latency;
        }
        return mean / m_LatencyHistory.Count;
    }

    private float TimeDifferenceVariance()
    {
        float variance = 0;
        foreach (float difference in m_TimeDifferenceHistory)
        {
            variance += Mathf.Abs(difference);
        }
        variance = variance / m_TimeDifferenceHistory.Count;
        return variance;
    }
}
