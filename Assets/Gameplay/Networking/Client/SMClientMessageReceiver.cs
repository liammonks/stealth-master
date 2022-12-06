using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Client
{

    public class SMClientMessageReceiver
    {

        private SMClient m_SMClient;

        /// <summary>
        /// Constructor
        /// Stores the client and starts listening for messages
        /// </summary>
        /// <param name="client"></param>
        public SMClientMessageReceiver(SMClient smClient)
        {
            m_SMClient = smClient;
            m_SMClient.Client.MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Destructor
        /// Stops listening for messages
        /// </summary>
        ~SMClientMessageReceiver()
        {
            m_SMClient.Client.MessageReceived -= OnMessageReceived;
        }

        /// <summary>
        /// Receives messages from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            using (Message message = args.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch ((ServerTag)message.Tag)
                    {
                        case ServerTag.ClientConnected:
                            OnClientConnected(reader.ReadSerializable<ClientConnectedResponse>());
                            break;
                        case ServerTag.UnitSpawned:
                            OnUnitSpawned(reader.ReadSerializable<SpawnUnitPacket>());
                            break;
                        case ServerTag.ClientDisconnected:
                            OnClientDisconnected(reader.ReadSerializable<ClientDisconnected>());
                            break;
                        default:
                            Debug.LogError("Message received with unknown tag!");
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Message recieved after connecting to a server
        /// </summary>
        /// <param name="data"></param>
        private void OnClientConnected(ClientConnectedResponse data)
        {
            RequestUnit();
        }

        /// <summary>
        /// Sends a message to server, requesting a unit
        /// </summary>
        private void RequestUnit()
        {
            SpawnUnitRequest spawnUnitRequest = new SpawnUnitRequest();
            spawnUnitRequest.PrefabIndex = 0;
            spawnUnitRequest.Position = PlayerManager.Instance.Unit.transform.position;

            m_SMClient.MessageSender.QueueMessage(ClientTag.SpawnUnitRequest, spawnUnitRequest);
        }

        /// <summary>
        /// Message received from server when a unit is spawned
        /// </summary>
        /// <param name="data"></param>
        private void OnUnitSpawned(SpawnUnitPacket data)
        {
            if (data.ClientID == m_SMClient.ID)
            {
                // Our unit was spawned on the server, we dont need to instantiate a new one, just add ClientUnit data to the dictionary
                m_SMClient.UnitData.ClientUnits.Add(m_SMClient.ID, new NetworkUnitData.ClientUnit(m_SMClient.ID, PlayerManager.Instance.Unit));
            }
            else
            {
                // Server spawned another clients unit, spawn the unit locally
                m_SMClient.UnitData.SpawnUnit(data.ClientID, data.PrefabIndex, data.Position);
            }
        }

        /// <summary>
        /// Message received from server when a client disconnects
        /// </summary>
        /// <param name="clientDisconnected"></param>
        private void OnClientDisconnected(ClientDisconnected clientDisconnected)
        {
            m_SMClient.UnitData.DestroyUnit(clientDisconnected.ClientID);
        }
    }

}