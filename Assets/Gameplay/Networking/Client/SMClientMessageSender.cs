using DarkRift;
using Network.Client;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Network.Client.SMClientMessageSender;

namespace Network.Client
{

    public class SMClientMessageSender
    {
        private const float MessageSendRate = 60;
        private const float MessageSendInterval = 1 / MessageSendRate;

        private SMClient m_SMClient;
        private Queue<Message> m_MessageQueue = new Queue<Message>();

        public SMClientMessageSender(SMClient smClient)
        {
            m_SMClient = smClient;
            m_SMClient.StartCoroutine(SendMessageQueue());
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
                    m_SMClient.Client.SendMessage(message, SendMode.Reliable);
                }
            }

            yield return new WaitForSecondsRealtime(MessageSendInterval);
            m_SMClient.StartCoroutine(SendMessageQueue());
        }
    }

}