using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;

public class ClientMessageReceiver : MonoBehaviour
{
    private UnityClient m_Client;
    private ClientSyncTime m_SyncTime;

    private void Awake()
    {
        m_SyncTime = GetComponent<ClientSyncTime>();
        m_Client = GetComponent<UnityClient>();
        m_Client.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        using Message message = args.GetMessage();
        switch ((Tag)args.Tag)
        {
            case Tag.ClientConnected:
                NetworkPlayers.Instance.RegisterPlayer(message.Deserialize<ClientConnected>());
                break;
            case Tag.ClientDisconnected:
                NetworkPlayers.Instance.RemovePlayer(message.Deserialize<ClientDisconnected>());
                break;
            case Tag.SyncTime:
                m_SyncTime.OnSyncTimeReceived(message.Deserialize<SyncTime>());
                break;
            case Tag.LatencyUpdate:
                NetworkManager.RegisterLatency(message.Deserialize<LatencyUpdate>());
                break;
        }
    }

}
