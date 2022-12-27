using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using Network.Shared;
using UnityEngine.UI;
using Sirenix.Utilities;
using System.Linq;

namespace Network.Server
{

    public class Server : XmlUnityServer
    {
        // Properties
        public IClient[] AllClients => Server.ClientManager.GetAllClients();

        // Shared
        [HideInInspector]
        public NetworkUnitData UnitData;
        [HideInInspector]
        public NetworkInputBuffer InputBuffer;

        [SerializeField]
        public Image m_Image;

        // Server
        public ServerMessageReceiver MessageReceiver;
        public ServerMessageSender MessageSender;
        public ServerTime Time;

        private void Awake()
        {
            UnitData = GetComponentInChildren<NetworkUnitData>();
            InputBuffer = GetComponentInChildren<NetworkInputBuffer>();
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
            m_Image.color = Time.SimulationTime % 5 == 0 ? Color.white : Color.black;
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
        /// <param name="requestingClient"></param>
        private void SendUnitData(IClient requestingClient)
        {
            foreach (IClient client in AllClients)
            {
                if (!UnitData.ClientUnits.ContainsKey(client.ID) || client.ID == requestingClient.ID)
                {
                    continue;
                }

                SpawnUnitPacket spawnUnitPacket = new SpawnUnitPacket();
                spawnUnitPacket.ClientID = client.ID;
                spawnUnitPacket.PrefabIndex = UnitData.ClientUnitPrefabIndicies[client.ID];
                spawnUnitPacket.Position = UnitData.ClientUnits[client.ID].transform.position;

                MessageSender.QueueMessage(requestingClient, ServerTag.UnitSpawned, spawnUnitPacket);
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