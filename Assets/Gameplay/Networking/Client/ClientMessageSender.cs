using DarkRift;
using Network.Client;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Client
{

    public class ClientMessageSender
    {
        private const float m_ArtificialLatency = 0.0f;

        private const float MessageSendRate = 60;
        private const float MessageSendInterval = 1 / MessageSendRate;

        private Client m_Client;
        private Queue<Message> m_MessageQueue = new Queue<Message>();

        public ClientMessageSender(Client client)
        {
            m_Client = client;
            m_Client.StartCoroutine(SendMessageQueue());
        }

        #region Instant

        public void SendMessage<T>(ClientTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                //using (Message message = Message.Create((ushort)tag, writer))
                //{
                //    m_Client.UnityClient.SendMessage(message, SendMode.Reliable);
                //}
                m_Client.StartCoroutine(SendMessageDelayed(Message.Create((ushort)tag, writer)));
            }
        }

        private IEnumerator SendMessageDelayed(Message message)
        {
            yield return new WaitForSeconds(m_ArtificialLatency);
            m_Client.UnityClient.SendMessage(message, SendMode.Reliable);
        }

        #endregion

        #region Queue

        public void QueueMessage<T>(ClientTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                //m_MessageQueue.Enqueue(Message.Create((ushort)tag, writer));
                m_Client.StartCoroutine(QueueMessageDelayed(Message.Create((ushort)tag, writer)));
            }
        }
    
        private IEnumerator QueueMessageDelayed(Message message)
        {
            yield return new WaitForSeconds(m_ArtificialLatency);
            m_MessageQueue.Enqueue(message);
        }

        private IEnumerator SendMessageQueue()
        {
            while (m_MessageQueue.Count > 0)
            {
                using (Message message = m_MessageQueue.Dequeue())
                {
                    m_Client.UnityClient.SendMessage(message, SendMode.Reliable);
                }
            }

            yield return new WaitForSecondsRealtime(MessageSendInterval);
            m_Client.StartCoroutine(SendMessageQueue());
        }

        #endregion

    }

}