using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using Network.Shared;

namespace Network.Server
{

    public class SMServer : XmlUnityServer
    {
        private const ushort INPUT_BUFFER_SIZE = 60;

        private NetworkUnitData m_UnitData;
        private uint m_CurrentTick = 0;


        private void Awake()
        {
            m_UnitData = GetComponentInChildren<NetworkUnitData>();
        }

        void Start()
        {
            Server.ClientManager.ClientConnected += OnClientConnected;
            Server.ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs args)
        {
            ClientConnectedResponse response = new ClientConnectedResponse();
            response.CurrentTick = m_CurrentTick;

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(response);
                using (Message message = Message.Create((ushort)Tag.ClientConnectedResponse, writer))
                {
                    args.Client.SendMessage(message, SendMode.Reliable);
                }
            }

            args.Client.MessageReceived += OnMessageReceived;
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
                using (Message message = Message.Create((ushort)Tag.ClientDisconnected, writer))
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
                    switch ((Tag)message.Tag)
                    {
                        case Tag.SpawnUnitRequest:
                            OnSpawnUnitRequest(args.Client, reader.ReadSerializable<SpawnUnitRequest>());
                            break;
                        case Tag.MovementInput:
                            OnMovementInputPacket(args.Client, reader.ReadSerializable<FloatInputPacket>());
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
                using (Message message = Message.Create((ushort)Tag.SpawnUnitResponse, writer))
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
                    using (Message message = Message.Create((ushort)Tag.SpawnUnitResponse, writer))
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }
        }

        #region Receive Input

        private void OnMovementInputPacket(IClient client, FloatInputPacket data)
        {
            if (!m_UnitData.ClientUnits.ContainsKey(client.ID)) { return; }
            Debug.Log($"Movement: {data.value}");
            m_UnitData.ClientUnits[client.ID].Unit.Input.Movement = data.value;
        }

        #endregion
    }

}