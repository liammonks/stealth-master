using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Shared
{

    public class NetworkRollback : SimulationBehaviour
    {
        private NetworkUnitData m_UnitData;

        private Dictionary<float, List<UnitStatePacket>> m_UnitStateHistory = new Dictionary<float, List<UnitStatePacket>>();

        protected override void OnAwake()
        {
            m_Order = SimulationOrder.Rollback;
        }

        public override void Simulate(float timeStep)
        {

        }

        private void StoreUnitData()
        {
            foreach (KeyValuePair<ushort, Unit> unitData in m_UnitData.ClientUnits)
            {
                ushort clientID = unitData.Key;
                Unit unit = unitData.Value;

                UnitStatePacket packet = new UnitStatePacket();
                packet.clientID = clientID;
            }
            
        }

    }

}