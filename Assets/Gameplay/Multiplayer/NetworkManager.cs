using System.Collections;
using Mirror;
using UnityEngine;

public enum NetworkType {
    Client,
    Host,
    Server
}

public class NetworkManager : Mirror.NetworkManager
{
    public static NetworkType networkType = NetworkType.Server;

    public delegate void OnNetworkUpdate();
    public static event OnNetworkUpdate onNetworkUpdate;
    private const float networkUpdateInterval = 0.01f;

    public override void Start()
    {
        base.Start();
        switch (networkType)
        {
            case NetworkType.Client:
                StartClient();
                break;
            case NetworkType.Host:
                StartHost();
                break;
            case NetworkType.Server:
                StartServer();
                break;
        }
    }

    public override void OnStartServer() {
        // Disable player for server
        if(mode == NetworkManagerMode.ServerOnly)
        {
            UnitHelper.Player.gameObject.SetActive(false);
        }
        StartCoroutine(NetworkUpdate());
    }

    public override void OnServerAddPlayer(NetworkConnection client)
    {
        base.OnServerAddPlayer(client);
        NetworkPlayer clientPlayer = client.identity.GetComponent<NetworkPlayer>();
        clientPlayer.TargetLoadLevel(client, LevelManager.sceneName);
        ServerPlayer.Instance.AddPlayer(clientPlayer);
    }

    private IEnumerator NetworkUpdate()
    {
        yield return new WaitForSecondsRealtime(networkUpdateInterval);
        if(onNetworkUpdate != null) onNetworkUpdate.Invoke();
        StartCoroutine(NetworkUpdate());
    }

}
