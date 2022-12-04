using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityClient))]
public class SMClient : MonoBehaviour
{

    private UnityClient m_Client;
    private NetworkUnitData m_UnitData;
    private uint m_CurrentTick = 0;

    private void Awake()
    {
        m_UnitData = GetComponentInChildren<NetworkUnitData>();

        m_Client = GetComponent<UnityClient>();
        m_Client.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        using (Message message = args.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                switch ((Tags)message.Tag)
                {
                    case Tags.ClientConnectedResponse:
                        OnClientConnectedResponse(reader.ReadSerializable<ClientConnectedResponse>());
                        break;
                    case Tags.SpawnUnitResponse:
                        OnSpawnUnitResponse(reader.ReadSerializable<SpawnUnitResponse>());
                        break;
                    case Tags.ClientDisconnected:
                        OnClientDisconnected(reader.ReadSerializable<ClientDisconnected>());
                        break;
                    default:
                        Debug.LogError("Message received with unknown tag!");
                        break;
                }
            }
        }
    }

    private void OnClientConnectedResponse(ClientConnectedResponse data)
    {
        m_CurrentTick = data.CurrentTick;
        RequestUnit();
    }

    private void RequestUnit()
    {
        SpawnUnitRequest spawnUnitRequest = new SpawnUnitRequest();
        spawnUnitRequest.PrefabIndex = 0;
        spawnUnitRequest.Position = PlayerManager.Instance.Unit.transform.position;

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(spawnUnitRequest);
            using (Message message = Message.Create((ushort)Tags.SpawnUnitRequest, writer))
            {
                m_Client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    private void OnSpawnUnitResponse(SpawnUnitResponse data)
    {
        if (data.ClientID == m_Client.ID)
        {
            m_UnitData.ClientUnits.Add(m_Client.ID, new NetworkUnitData.ClientUnit(m_Client.ID, PlayerManager.Instance.Unit));
        }
        else
        {
            m_UnitData.SpawnUnit(data.ClientID, data.PrefabIndex, data.Position);
        }
    }

    private void SendInput()
    {
        if (!m_UnitData.ClientUnits.ContainsKey(m_Client.ID)) { return; }
        UnitInput input = PlayerManager.Instance.Unit.Input;

        InputPacket packet = new InputPacket();
        packet.Movement = input.Movement;
        packet.Running = input.Running;

        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(packet);
            using (Message message = Message.Create((ushort)Tags.InputPacket, writer))
            {
                m_Client.SendMessage(message, SendMode.Reliable);
            }
        }
    }

    private void OnClientDisconnected(ClientDisconnected clientDisconnected)
    {
        m_UnitData.DestroyUnit(clientDisconnected.ClientID);
    }
}
