using DarkRift;
using DarkRift.Server;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Server
{

    public class ServerMessageReceiver
    {
        
        private Server m_Server;
        private NetworkPhysicsHistory m_PhysicsHistory;

        public ServerMessageReceiver(Server server)
        {
            m_Server = server;
            m_PhysicsHistory = m_Server.GetComponentInChildren<NetworkPhysicsHistory>();
        }

        public void RegisterClient(IClient client)
        {
            client.MessageReceived += OnMessageReceived;
        }

        public void UnregisterClient(IClient client)
        {
            client.MessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            using (Message message = args.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch ((ClientTag)message.Tag)
                    {
                        case ClientTag.SimulationTimeRequest:
                            OnSimulationTimeRequest(args.Client, reader.ReadSerializable<SimulationTimeSync>());
                            break;
                        case ClientTag.SpawnUnitRequest:
                            OnSpawnUnitRequest(args.Client, reader.ReadSerializable<SpawnUnitRequest>());
                            break;
                        case ClientTag.BoolInput:
                            OnBoolInputReceived(args.Client, reader.ReadSerializable<BoolInputPacket>());
                            break;
                        case ClientTag.FloatInput:
                            OnFloatInputReceived(args.Client, reader.ReadSerializable<FloatInputPacket>());
                            break;
                        default:
                            Debug.LogError("Message received with unknown tag!");
                            break;
                    }
                }
            }
        }

        private void OnSimulationTimeRequest(IClient sender, SimulationTimeSync simulationTimeSync)
        {
            simulationTimeSync.T1 = m_Server.Time.SimulationTime;
            m_Server.MessageSender.SendMessage(sender, ServerTag.SimulationTimeResponse, simulationTimeSync);
        }

        private void OnSpawnUnitRequest(IClient sender, SpawnUnitRequest data)
        {
            if (m_Server.UnitData.ClientUnits.ContainsKey(sender.ID)) { return; }

            m_Server.UnitData.SpawnUnit(sender.ID, data.PrefabIndex, data.Position);

            // Tell all clients to initialise this unit
            SpawnUnitPacket spawnUnitPacket = new SpawnUnitPacket();
            spawnUnitPacket.ClientID = sender.ID;
            spawnUnitPacket.PrefabIndex = data.PrefabIndex;
            spawnUnitPacket.Position = data.Position;

            m_Server.MessageSender.QueueMessage(m_Server.AllClients, ServerTag.UnitSpawned, spawnUnitPacket);
        }

        #region Receive Input

        private void OnBoolInputReceived(IClient sender, BoolInputPacket input)
        {
            if (!m_Server.UnitData.ClientUnits.ContainsKey(sender.ID)) { return; }
            Debug.Log($"Input Message Received");
            Debug.Log($"Client timestamp {input.simulationTime}\nServer timestamp {m_Server.Time.SimulationTime}");
            m_Server.InputBuffer.RegisterBoolInput(input);
        }

        private void OnFloatInputReceived(IClient sender, FloatInputPacket input)
        {
            if (!m_Server.UnitData.ClientUnits.ContainsKey(sender.ID)) { return; }
            m_Server.InputBuffer.RegisterFloatInput(input);
        }

        #endregion

    }

}