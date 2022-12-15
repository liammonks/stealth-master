using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using Network.Shared;
using UnityEngine.UI;

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

        [SerializeField]
        private Image m_Image;

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
            m_Image.color = Time.SimulationTime % 5 == 0 ? Color.white : Color.black;
        }

    }

}