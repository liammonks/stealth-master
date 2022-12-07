using DarkRift;
using Network.Client;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Network.Client.ClientMessageSender;

namespace Network.Client
{

    public class ClientMessageSender
    {
        private const float MessageSendRate = 60;
        private const float MessageSendInterval = 1 / MessageSendRate;

        private Client m_Client;
        private Queue<Message> m_MessageQueue = new Queue<Message>();

        public ClientMessageSender(Client client)
        {
            m_Client = client;
            m_Client.StartCoroutine(SendMessageQueue());
        }

        public void QueueMessage<T>(ClientTag tag, T serializable) where T : IDarkRiftSerializable
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write<T>(serializable);
                m_MessageQueue.Enqueue(Message.Create((ushort)tag, writer));
            }
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
    }

}