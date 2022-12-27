using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Server;
using System.Collections;
using UnityEngine;

public class ClientMessageSender : MonoBehaviour
{
    private static ClientMessageSender m_Instance;
    private static UnityClient m_Client;

    public static void SendMessage<T>(Tag tag, T serializable) where T : IDarkRiftSerializable
    {
        if (NetworkManager.ArtificialLatency != 0)
        {
            m_Instance.StartCoroutine(m_Instance.SendMessageDelayed(tag, serializable));
            return;
        }

        using DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write(serializable);
        using Message message = Message.Create((ushort)tag, writer);
        m_Client.SendMessage(message, SendMode.Reliable);
    }

    private IEnumerator SendMessageDelayed<T>(Tag tag, T serializable) where T : IDarkRiftSerializable
    {
        yield return new WaitForSeconds(NetworkManager.ArtificialLatency);

        using DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write(serializable);
        using Message message = Message.Create((ushort)tag, writer);
        m_Client.SendMessage(message, SendMode.Reliable);
    }

    private void Awake()
    {
        m_Instance = this;
        m_Client = GetComponent<UnityClient>();
    }
}
