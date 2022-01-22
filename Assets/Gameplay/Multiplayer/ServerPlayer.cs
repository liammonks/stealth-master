using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ServerPlayer : NetworkBehaviour
{
    public static ServerPlayer Instance;

    public List<NetworkPlayer> networkPlayers = new List<NetworkPlayer>();

    private void Awake() {
        if(Instance != null)
        {
            Debug.LogError("Two instances of ServerPlayer found");
            return;
        }
        Instance = this;
    }

    public void AddPlayer(NetworkPlayer player)
    {
        networkPlayers.Add(player);
    }

}
