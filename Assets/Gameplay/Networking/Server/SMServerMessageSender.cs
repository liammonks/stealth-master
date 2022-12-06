using DarkRift;
using DarkRift.Server;
using Network.Client;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Server
{

    public class SMServerMessageSender
    {
        private struct ClientMessage
        {
            public IEnumerable Clients;
            public Message Message;

            public ClientMessage(IEnumerable clients, Message message)
            {
                Clients = clients;
                Message = message;
            }
        }

        private const float MessageSendRate = 60;
        private const float MessageSendInterval = 1 / MessageSendRate;

        private SMServer m_SMServer;
        private Queue<ClientMessage> m_MessageQueue = new Queue<ClientMessage>();

        public SMServerMessageSender(SMServer smServer)
        {
            m_SMServer = smServer;
            m_SMServer.StartCoroutine(SendMessageQueue());
        }

        public void QueueMessage<T>(IClient client, ServerTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                m_MessageQueue.Enqueue(new ClientMessage(new IClient[] { client }, Message.Create((ushort)tag, writer)));
            }
        }

        public void QueueMessage<T>(IEnumerable clients, ServerTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                m_MessageQueue.Enqueue(new ClientMessage(clients, Message.Create((ushort)tag, writer)));
            }
        }

        private IEnumerator SendMessageQueue()
        {
            while (m_MessageQueue.Count > 0)
            {
                ClientMessage clientMessage = m_MessageQueue.Dequeue();
                using (Message message = clientMessage.Message)
                {
                    foreach (IClient client in clientMessage.Clients)
                    {
                        client.SendMessage(message, SendMode.Reliable);
                    }
                }
            }

            yield return new WaitForSecondsRealtime(MessageSendInterval);
            m_SMServer.StartCoroutine(SendMessageQueue());
        }
    }

}