using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Client
{

    public class ClientTime
    {
        public double SimulationTime => Time.fixedTimeAsDouble + m_SimulationTimeOffset;

        private double m_SimulationTimeOffset;

        public void InitialiseSimulationTime(double simulationTime)
        {
            m_SimulationTimeOffset = simulationTime - Time.fixedTimeAsDouble;
        }
    }

}