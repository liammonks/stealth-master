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
        // Properties
        public ushort ID => Client.ID;

        // Shared
        public NetworkUnitData UnitData;

        // Client
        public UnityClient Client;
        public SMClientMessageReceiver MessageReceiver;
        public SMClientMessageSender MessageSender;

        private SMClientInput m_ClientInput;

        private void Awake()
        {
            Client = GetComponent<UnityClient>();
            UnitData = GetComponentInChildren<NetworkUnitData>();

            MessageReceiver = new SMClientMessageReceiver(this);
            MessageSender = new SMClientMessageSender(this);

            m_ClientInput = new SMClientInput(this);
        }

    }

}