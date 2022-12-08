using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using Network.Shared;

namespace Network.Client
{

    [RequireComponent(typeof(UnityClient))]
    public class Client : MonoBehaviour
    {
        // Properties
        public ushort ID => UnityClient.ID;

        // Shared
        [HideInInspector]
        public NetworkUnitData UnitData;

        // Client
        [HideInInspector]
        public UnityClient UnityClient;
        public ClientMessageReceiver MessageReceiver;
        public ClientMessageSender MessageSender;
        public ClientTime Time;

        private ClientInput m_ClientInput;

        private void Awake()
        {
            UnityClient = GetComponent<UnityClient>();
            UnitData = GetComponentInChildren<NetworkUnitData>();

            MessageReceiver = new ClientMessageReceiver(this);
            MessageSender = new ClientMessageSender(this);
            Time = new ClientTime(this);

            m_ClientInput = new ClientInput(this);
        }

        private void FixedUpdate()
        {
            //Debug.Log("--SimulationTime: " + Time.SimulationTime);
        }

    }

}