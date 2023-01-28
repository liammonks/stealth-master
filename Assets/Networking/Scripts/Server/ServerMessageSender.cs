using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Collections;
using UnityEngine;

public class ServerMessageSender : MonoBehaviour
{
    private static ServerMessageSender m_Instance;
    private static DarkRiftServer m_Server;

    private void Start()
    {
        m_Instance = this;
        m_Server = GetComponent<ServerConnection>().Server;
    }

    public static void SendMessage<T>(Tag tag, T serializable) where T : IDarkRiftSerializable
    {
        IClient[] clients = m_Server.ClientManager.GetAllClients();

        if (NetworkManager.ArtificialLatency != 0)
        {
            m_Instance.StartCoroutine(m_Instance.SendMessageDelayed(clients, tag, serializable));
            return;
        }

        using DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write(serializable);
        using Message message = Message.Create((ushort)tag, writer);
        foreach (IClient client in clients)
        {
            client.SendMessage(message, SendMode.Reliable);
        }
    }

    public static void SendMessage<T>(IClient client, Tag tag, T serializable) where T : IDarkRiftSerializable
    {
        if (NetworkManager.ArtificialLatency != 0)
        {
            m_Instance.StartCoroutine(m_Instance.SendMessageDelayed(client, tag, serializable));
            return;
        }

        using DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write(serializable);
        using Message message = Message.Create((ushort)tag, writer);
        client.SendMessage(message, SendMode.Reliable);
    }

    public static void SendMessage<T>(IClient[] clients, Tag tag, T serializable) where T : IDarkRiftSerializable
    {
        if (NetworkManager.ArtificialLatency != 0)
        {
            m_Instance.StartCoroutine(m_Instance.SendMessageDelayed(clients, tag, serializable));
            return;
        }

        using DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write(serializable);
        using Message message = Message.Create((ushort)tag, writer);
        foreach (IClient client in clients)
        {
            client.SendMessage(message, SendMode.Reliable);
        }
    }

    private IEnumerator SendMessageDelayed<T>(IClient client, Tag tag, T serializable) where T : IDarkRiftSerializable
    {
        yield return new WaitForSeconds(NetworkManager.ArtificialLatency);
        using DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write(serializable);
        using Message message = Message.Create((ushort)tag, writer);
        client.SendMessage(message, SendMode.Reliable);
    }

    private IEnumerator SendMessageDelayed<T>(IClient[] clients, Tag tag, T serializable) where T : IDarkRiftSerializable
    {
        yield return new WaitForSeconds(NetworkManager.ArtificialLatency);
        using DarkRiftWriter writer = DarkRiftWriter.Create();
        writer.Write(serializable);
        using Message message = Message.Create((ushort)tag, writer);
        foreach (IClient client in clients)
        {
            client.SendMessage(message, SendMode.Reliable);
        }
    }

}
