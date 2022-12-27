using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server.Unity;
using DarkRift.Server;

public class ServerMessageReceiver : MonoBehaviour
{
    private DarkRiftServer m_Server;

    private void Start()
    {
        m_Server = GetComponent<XmlUnityServer>().Server;
        m_Server.ClientManager.ClientConnected += OnClientConnected;
        m_Server.ClientManager.ClientDisconnected += OnClientDisconnected;
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs args)
    {
        args.Client.MessageReceived += OnMessageReceived;

        ClientConnected packet = new ClientConnected();
        packet.clientID = args.Client.ID;
        ServerMessageSender.SendMessage(Tag.ClientConnected, packet);
        NetworkPlayers.Instance.RegisterPlayer(packet);
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs args)
    {
        args.Client.MessageReceived -= OnMessageReceived;

        ClientDisconnected packet = new ClientDisconnected();
        packet.clientID = args.Client.ID;
        ServerMessageSender.SendMessage(Tag.ClientDisconnected, packet);
        NetworkPlayers.Instance.RemovePlayer(packet);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        using Message message = args.GetMessage();
        switch ((Tag)args.Tag)
        {
            case Tag.SyncTime:
                OnSyncTime(args.Client, message.Deserialize<SyncTime>());
                break;
            case Tag.LatencyUpdate:
                OnLatencyUpdate(message.Deserialize<LatencyUpdate>());
                break;
            case Tag.ClientMoveInput:
                OnClientMoveInput(args.Client, message.Deserialize<ClientMoveInput>());
                break;
        }
    }

    private void OnSyncTime(IClient client, SyncTime packet)
    {
        packet.serverReceiveTime = Simulation.Time;
        ServerMessageSender.SendMessage(client, Tag.SyncTime, packet);
    }

    private void OnLatencyUpdate(LatencyUpdate packet)
    {
        NetworkManager.RegisterLatency(packet);
        ServerMessageSender.SendMessage(Tag.LatencyUpdate, packet);
    }

    private void OnClientMoveInput(IClient client, ClientMoveInput packet)
    {
        NetworkPlayers.Instance.Players[client.ID].Position += packet.moveInput;
    }
}
