#define USE_PARRELSYNC
#if USE_PARRELSYNC
using ParrelSync;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network.Shared
{

    public class NetworkManager : MonoBehaviour
    {
#if USE_PARRELSYNC
        public static NetworkType NetworkType => ClonesManager.GetArgument() == "server" ? NetworkType.Server : NetworkType.Client;
#else
    public static NetworkType NetworkType => NetworkType.Client;
#endif

        [SerializeField]
        private bool m_Connect = true;

        [SerializeField]
        private GameObject m_ClientPrefab;

        [SerializeField]
        private GameObject m_ServerPrefab;

        private GameObject m_NetworkObject;

        private void Awake()
        {
            if (!m_Connect) { return; }

            if (NetworkType == NetworkType.Server)
            {
                StartServer();
            }
            else
            {
                StartClient();
            }
        }

        private void StartClient()
        {
            m_NetworkObject = Instantiate(m_ClientPrefab, transform);
            m_NetworkObject.transform.SetParent(null);
            m_NetworkObject.name = "Client";
        }

        private void StartServer()
        {
            m_NetworkObject = Instantiate(m_ServerPrefab, transform);
            m_NetworkObject.transform.SetParent(null);
            m_NetworkObject.name = "Server";
        }

    }

}