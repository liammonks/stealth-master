using DarkRift;
using DarkRift.Server;
using Network.Client;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Server
{

    public class ServerMessageSender
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

        private const float m_ArtificialLatency = 0.1f;

        private const float MessageSendRate = 60;
        private const float MessageSendInterval = 1 / MessageSendRate;

        private Server m_Server;
        private Queue<ClientMessage> m_MessageQueue = new Queue<ClientMessage>();

        public ServerMessageSender(Server smServer)
        {
            m_Server = smServer;
            m_Server.StartCoroutine(SendMessageQueue());
        }

        #region Instant

        public void SendMessage<T>(IClient client, ServerTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                using (Message message = Message.Create((ushort)tag, writer))
                {
                    client.SendMessage(message, SendMode.Reliable);
                }

                //m_Server.StartCoroutine(SendMessageDelayed(client, Message.Create((ushort)tag, writer)));
            }
        }

        private IEnumerator SendMessageDelayed(IClient client, Message message)
        {
            yield return new WaitForSeconds(m_ArtificialLatency);
            client.SendMessage(message, SendMode.Reliable);
        }

        #endregion

        #region Queue

        public void QueueMessage<T>(IClient client, ServerTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                m_MessageQueue.Enqueue(new ClientMessage(new IClient[] { client }, Message.Create((ushort)tag, writer)));
                //m_Server.StartCoroutine(QueueMessageDelayed(new ClientMessage(new IClient[] { client }, Message.Create((ushort)tag, writer))));
            }
        }

        public void QueueMessage<T>(IEnumerable clients, ServerTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                m_MessageQueue.Enqueue(new ClientMessage(clients, Message.Create((ushort)tag, writer)));
                //m_Server.StartCoroutine(QueueMessageDelayed(new ClientMessage(clients, Message.Create((ushort)tag, writer))));
            }
        }

        private IEnumerator QueueMessageDelayed(ClientMessage clientMessage)
        {
            yield return new WaitForSeconds(m_ArtificialLatency);
            m_MessageQueue.Enqueue(clientMessage);
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
            m_Server.StartCoroutine(SendMessageQueue());
        }

        #endregion

    }

}