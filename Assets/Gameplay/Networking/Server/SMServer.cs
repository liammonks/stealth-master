using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;

public class SMServer : XmlUnityServer
{
    private const ushort INPUT_BUFFER_SIZE = 60;

    private NetworkUnitData m_UnitData;
    private Dictionary<ushort, InputPacket[]> m_InputBuffer = new Dictionary<ushort, InputPacket[]>();
    private uint m_CurrentTick = 0;


    private void Awake()
    {
        m_UnitData = GetComponentInChildren<NetworkUnitData>();
    }

    void Start()
    {
        Server.ClientManager.ClientConnected += OnClientConnected;
        Server.ClientManager.ClientDisconnected += OnClientDisconnected;
        TickMachine.Register(TickOrder.SMServer, OnTick);
    }

    private void OnDestroy()
    {
        TickMachine.Register(TickOrder.SMClient, OnTick);
    }

    public void OnTick()
    {
        foreach (IClient client in Server.ClientManager.GetAllClients())
        {
            if (!m_UnitData.ClientUnits.ContainsKey(client.ID)) { continue; }
            uint bufferIndex = (m_CurrentTick - (INPUT_BUFFER_SIZE / 2)) % INPUT_BUFFER_SIZE;
            InputPacket inputPacket = m_InputBuffer[client.ID][bufferIndex];
            if (inputPacket != null)
            {
                UnitInput unitInput = m_UnitData.ClientUnits[client.ID].Unit.Input;
                unitInput.TickData.Movement = inputPacket.Movement;
                unitInput.TickData.Running = inputPacket.Running;
                //Debug.Log(inputPacket.Movement);
            }
            else
            {
                //Debug.LogError("INPUT PACKET NULL");
            }
        }

        m_CurrentTick++;
    }

    private void OnClientConnected(object sender, ClientConnectedEventArgs args)
    {
        ClientConnectedResponse response = new ClientConnectedResponse();
        response.CurrentTick = m_CurrentTick;

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(response);
            using (Message message = Message.Create((ushort)Tags.ClientConnectedResponse, writer))
            {
                args.Client.SendMessage(message, SendMode.Reliable);
            }
        }

        args.Client.MessageReceived += OnMessageReceived;
        m_InputBuffer.Add(args.Client.ID, new InputPacket[INPUT_BUFFER_SIZE]);
        SendUnitData(args.Client);
    }

    private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs args)
    {
        m_UnitData.DestroyUnit(args.Client.ID);

        ClientDisconnected clientDisconnected = new ClientDisconnected();
        clientDisconnected.ClientID = args.Client.ID;

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(clientDisconnected);
            using (Message message = Message.Create((ushort)Tags.ClientDisconnected, writer))
            {
                foreach (IClient client in Server.ClientManager.GetAllClients())
                {
                    client.SendMessage(message, SendMode.Reliable);
                }
            }
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        using (Message message = args.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                switch ((Tags)message.Tag)
                {
                    case Tags.SpawnUnitRequest:
                        OnSpawnUnitRequest(args.Client, reader.ReadSerializable<SpawnUnitRequest>());
                        break;
                    case Tags.InputPacket:
                        OnInputPacket(args.Client, reader.ReadSerializable<InputPacket>());
                        break;
                    default:
                        Debug.LogError("Message received with unknown tag!");
                        break;
                }
            }
        }
    }

    private void OnSpawnUnitRequest(IClient sender, SpawnUnitRequest data)
    {
        if (m_UnitData.ClientUnits.ContainsKey(sender.ID)) { return; }

        m_UnitData.SpawnUnit(sender.ID, data.PrefabIndex, data.Position);

        // Tell all clients to initialise this unit
        SpawnUnitResponse response = new SpawnUnitResponse();
        response.ClientID = sender.ID;
        response.PrefabIndex = data.PrefabIndex;
        response.Position = data.Position;

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(response);
            using (Message message = Message.Create((ushort)Tags.SpawnUnitResponse, writer))
            {
                foreach (IClient client in Server.ClientManager.GetAllClients())
                {
                    client.SendMessage(message, SendMode.Reliable);
                }
            }
        }
    }

    private void SendUnitData(IClient client)
    {
        foreach (KeyValuePair<ushort, NetworkUnitData.ClientUnit> kvp in m_UnitData.ClientUnits)
        {
            SpawnUnitResponse response = new SpawnUnitResponse();
            response.ClientID = kvp.Key;
            response.PrefabIndex = kvp.Value.PrefabIndex;
            response.Position = kvp.Value.Unit.transform.position;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(response);
                using (Message message = Message.Create((ushort)Tags.SpawnUnitResponse, writer))
                {
                    client.SendMessage(message, SendMode.Reliable);
                }
            }
        }
    }

    private void OnInputPacket(IClient client, InputPacket data)
    {
        if (!m_UnitData.ClientUnits.ContainsKey(client.ID)) { return; }
        uint bufferIndex = m_CurrentTick % INPUT_BUFFER_SIZE;
        //Debug.Log($"Placing input data for client with ID {client.ID} in buffer index {bufferIndex}");
        m_InputBuffer[client.ID][bufferIndex] = data;
    }
}
