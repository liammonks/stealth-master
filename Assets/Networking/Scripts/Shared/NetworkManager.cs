using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static float ArtificialLatency = 0.0f;

    public static bool Active => m_Active;
    private static bool m_Active;

    public static NetworkType NetworkType => m_NetworkType;
    private static NetworkType m_NetworkType = NetworkType.Client;

    public static DarkRiftServer Server => m_Server;
    private static DarkRiftServer m_Server;

    public static UnityClient Client => m_Client;
    private static UnityClient m_Client;

    public static Dictionary<ushort, float> ClientLatency => m_ClientLatency;
    private static Dictionary<ushort, float> m_ClientLatency = new Dictionary<ushort, float>();

    private void Awake()
    {
        m_Client = FindObjectOfType<UnityClient>();
        m_Server = FindObjectOfType<XmlUnityServer>()?.Server;
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

}
