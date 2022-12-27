using DarkRift;
using Network.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{

    public class UnitStatePacket : IDarkRiftSerializable
    {
        public ushort clientID;

        // State Machine
        public float[] statesLastExecutionTimes;
        public UnitState currentState;
        public UnitState previousState;
        public UnitState lastFrameState;



        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(clientID);
        }

        public void Deserialize(DeserializeEvent e)
        {
            clientID = e.Reader.ReadUInt16();
            
        }
    }

}