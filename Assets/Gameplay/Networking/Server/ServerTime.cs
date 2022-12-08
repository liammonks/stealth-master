using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.CacheServer;
using UnityEngine;

namespace Network.Server
{

    public class ServerTime
    {
        public double SimulationTime => m_SimulationTime;
        private double m_SimulationTime => Time.fixedTimeAsDouble;
    }

}