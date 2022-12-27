using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer
{
    public Vector2Int Position;
}

public class NetworkPlayers : SingletonBehaviour<NetworkPlayers>
{
    public Dictionary<ushort, NetworkPlayer> Players => m_Players;
    private Dictionary<ushort, NetworkPlayer> m_Players = new Dictionary<ushort, NetworkPlayer>();
    
    public void RegisterPlayer(ClientConnected packet)
    {
        NetworkPlayer player = new NetworkPlayer();
        player.Position = Vector2Int.zero;

        m_Players.Add(packet.clientID, player);
    }

    public void RemovePlayer(ClientDisconnected packet)
    {
        m_Players.Remove(packet.clientID);
    }
}
