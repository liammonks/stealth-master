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

        public ServerMessageReceiver(Server server)
        {
            m_Server = server;
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
                        case ClientTag.SpawnUnitRequest:
                            OnSpawnUnitRequest(args.Client, reader.ReadSerializable<SpawnUnitRequest>());
                            break;
                        case ClientTag.MovementInput:
                            OnMovementInput(args.Client, reader.ReadSerializable<FloatInputPacket>());
                            break;
                        case ClientTag.RunningInput:
                            OnRunningInput(args.Client, reader.ReadSerializable<BoolInputPacket>());
                            break;
                        case ClientTag.JumpingInput:
                            OnJumpingInput(args.Client, reader.ReadSerializable<BoolInputPacket>());
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

        private void OnMovementInput(IClient sender, FloatInputPacket input)
        {
            if (!m_Server.UnitData.ClientUnits.ContainsKey(sender.ID)) { return; }
            m_Server.UnitData.ClientUnits[sender.ID].Unit.Input.Movement = input.value;
        }

        private void OnRunningInput(IClient sender, BoolInputPacket input)
        {
            if (!m_Server.UnitData.ClientUnits.ContainsKey(sender.ID)) { return; }
            m_Server.UnitData.ClientUnits[sender.ID].Unit.Input.Running = input.value;
        }

        private void OnJumpingInput(IClient sender, BoolInputPacket input)
        {
            if (!m_Server.UnitData.ClientUnits.ContainsKey(sender.ID)) { return; }
            m_Server.UnitData.ClientUnits[sender.ID].Unit.Input.Jumping = input.value;
        }

        #endregion

    }

}