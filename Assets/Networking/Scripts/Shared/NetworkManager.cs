using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static float ArtificialLatency = 0.0f;

    public static bool Active => m_Active;
    private static bool m_Active;

    public static NetworkType NetworkType => GetNetworkType();

    public static DarkRiftServer Server => m_ServerConnection.Server;
    private static ServerConnection m_ServerConnection;

    public static DarkRiftClient Client => m_ClientConnection.Client;
    private static ClientConnection m_ClientConnection;

    public static Dictionary<ushort, float> ClientLatency => m_ClientLatency;
    private static Dictionary<ushort, float> m_ClientLatency = new Dictionary<ushort, float>();

    private void Start()
    {
        m_ClientConnection = FindObjectOfType<ClientConnection>();
        m_ServerConnection = FindObjectOfType<ServerConnection>();
    }

    public static void RegisterLatency(LatencyUpdate packet)
    {
        if (m_ClientLatency.ContainsKey(packet.clientID))
        {
            m_ClientLatency[packet.clientID] = packet.latency;
        }
        else
        {
            m_ClientLatency.Add(packet.clientID, packet.latency);
        }
    }

    public static float AllClientAverageLatency()
    {
        if (m_ClientLatency.Count == 0) { return 0; }

        float average = 0;
        foreach (float latency in m_ClientLatency.Values)
        {
            average += latency;
        }
        return average / m_ClientLatency.Count;
    }

    private static NetworkType GetNetworkType()
    {
        NetworkType networkType = NetworkType.Offline;
        if (m_ClientConnection != null || NetworkSceneLoader.ClientActive) networkType = NetworkType.Client;
        if (m_ServerConnection != null || NetworkSceneLoader.ServerActive) networkType = NetworkType.Server;
        return networkType;
    }

}
