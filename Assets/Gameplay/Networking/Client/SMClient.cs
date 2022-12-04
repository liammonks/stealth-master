using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using Network.Shared;

namespace Network.Client
{

    [RequireComponent(typeof(UnityClient))]
    public class SMClient : MonoBehaviour
    {

        private UnityClient m_Client;
        private NetworkUnitData m_UnitData;

        private void Awake()
        {
            m_UnitData = GetComponentInChildren<NetworkUnitData>();

            m_Client = GetComponent<UnityClient>();
            m_Client.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            using (Message message = args.GetMessage())
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch ((Tag)message.Tag)
                    {
                        case Tag.ClientConnectedResponse:
                            OnClientConnectedResponse(reader.ReadSerializable<ClientConnectedResponse>());
                            break;
                        case Tag.SpawnUnitResponse:
                            OnSpawnUnitResponse(reader.ReadSerializable<SpawnUnitResponse>());
                            break;
                        case Tag.ClientDisconnected:
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
        private void OnClientConnectedResponse(ClientConnectedResponse data)
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

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(spawnUnitRequest);
                using (Message message = Message.Create((ushort)Tag.SpawnUnitRequest, writer))
                {
                    m_Client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        /// <summary>
        /// Message received from server when it spawns a unit
        /// </summary>
        /// <param name="data"></param>
        private void OnSpawnUnitResponse(SpawnUnitResponse data)
        {
            if (data.ClientID == m_Client.ID)
            {
                // Our unit was spawned on the server, we dont need to instantiate a new one, just add ClientUnit data to the dictionary
                m_UnitData.ClientUnits.Add(m_Client.ID, new NetworkUnitData.ClientUnit(m_Client.ID, PlayerManager.Instance.Unit));
            }
            else
            {
                // Server spawned another clients unit, this client should spawn it too
                m_UnitData.SpawnUnit(data.ClientID, data.PrefabIndex, data.Position);
            }
        }

        /// <summary>
        /// Message recieved from server when a client disconnects
        /// </summary>
        /// <param name="clientDisconnected"></param>
        private void OnClientDisconnected(ClientDisconnected clientDisconnected)
        {
            m_UnitData.DestroyUnit(clientDisconnected.ClientID);
        }
    }

}