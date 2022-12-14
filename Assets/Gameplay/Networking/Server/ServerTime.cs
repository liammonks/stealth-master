using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.CacheServer;
using UnityEngine;

namespace Network.Server
{

    public class ServerTime
    {
        public float SimulationTime => m_SimulationTime;
        private float m_SimulationTime => Time.fixedTime;
    }

}