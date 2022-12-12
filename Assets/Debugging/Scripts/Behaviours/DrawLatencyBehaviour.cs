using Network.Client;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Debugging.Behaviours
{

    public class DrawLatencyBehaviour : MonoBehaviour
    {
        public static DrawLatencyBehaviour Instance;

        [SerializeField]
        private Client m_Client;

        private DebugLabel m_Label;

        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Two instances of DebugClientLatency found");
                gameObject.SetActive(false);
                return;
            }

            Instance = this;
            m_Label = GetComponent<DebugLabel>();
        }

        private void Update()
        {
            m_Label.SetText($"Latency: {GetLatency()}");
        }

        private double GetLatency()
        {
            return (double)typeof(ClientTime).GetField("m_Latency", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m_Client.Time);
        }
    }

}