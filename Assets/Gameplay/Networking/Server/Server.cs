using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using Network.Shared;

namespace Network.Server
{

    public class Server : XmlUnityServer
    {
        // Properties
        public IClient[] AllClients => Server.ClientManager.GetAllClients();

        // Shared
        [HideInInspector]
        public NetworkUnitData UnitData;

        // Server
        public ServerMessageReceiver MessageReceiver;
        public ServerMessageSender MessageSender;
        public ServerTime Time;

        private void Awake()
        {
            UnitData = GetComponentInChildren<NetworkUnitData>();
            MessageReceiver = new ServerMessageReceiver(this);
            MessageSender = new ServerMessageSender(this);
            Time = new ServerTime();
        }

        void Start()
        {
            Server.ClientManager.ClientConnected += OnClientConnected;
            Server.ClientManager.ClientDisconnected += OnClientDisconnected;
        }

        private void FixedUpdate()
        {
            //Debug.Log("--SimulationTime: " + Time.SimulationTime);
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs args)
        {
            MessageReceiver.RegisterClient(args.Client);

            // Tell the client they have connected, providing the simulation time
            ClientConnectedResponse clientConnectedResponse = new ClientConnectedResponse();
            clientConnectedResponse.SimulationTime = Time.SimulationTime;
            MessageSender.QueueMessage(args.Client, ServerTag.ClientConnected, clientConnectedResponse);

            // Send the client current unit data
            SendUnitData(args.Client);
        }

        /// <summary>
        /// Sends all unit data to a client
        /// </summary>
        /// <param name="client"></param>
        private void SendUnitData(IClient client)
        {
            foreach (KeyValuePair<ushort, NetworkUnitData.ClientUnit> kvp in UnitData.ClientUnits)
            {
                SpawnUnitPacket spawnUnitPacket = new SpawnUnitPacket();
                spawnUnitPacket.ClientID = kvp.Key;
                spawnUnitPacket.PrefabIndex = kvp.Value.PrefabIndex;
                spawnUnitPacket.Position = kvp.Value.Unit.transform.position;

                MessageSender.QueueMessage(client, ServerTag.UnitSpawned, spawnUnitPacket);
            }
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs args)
        {
            MessageReceiver.UnregisterClient(args.Client);

            ClientDisconnected clientDisconnected = new ClientDisconnected();
            clientDisconnected.ClientID = args.Client.ID;
            MessageSender.QueueMessage(AllClients, ServerTag.ClientDisconnected, clientDisconnected);

            UnitData.DestroyUnit(args.Client.ID);
        }

    }

}