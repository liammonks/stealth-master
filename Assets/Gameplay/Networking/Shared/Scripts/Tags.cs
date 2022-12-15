using DarkRift;
using UnityEngine;

namespace Network.Shared
{

    public enum ClientTag
    {
        // Units
        SpawnUnitRequest,
        
        // Input
        MovementInput,
        RunningInput,
        JumpingInput,

        // Time
        SimulationTimeRequest
    }

    public enum ServerTag
    {
        ClientConnected,
        ClientDisconnected,
        UnitSpawned,

        // Time
        SimulationTimeResponse
    }

    #region Input

    public class FloatInputPacket : IDarkRiftSerializable
    {
        public ushort clientID;
        public float simulationTime;
        public float value;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(clientID);
            e.Writer.Write(simulationTime);
            e.Writer.Write(value);
        }

        public void Deserialize(DeserializeEvent e)
        {
            clientID = e.Reader.ReadUInt16();
            simulationTime = e.Reader.ReadSingle();
            value = e.Reader.ReadSingle();
        }
    }

    public class BoolInputPacket : IDarkRiftSerializable
    {
        public ushort clientID;
        public float simulationTime;
        public bool value;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(clientID);
            e.Writer.Write(simulationTime);
            e.Writer.Write(value);
        }

        public void Deserialize(DeserializeEvent e)
        {
            clientID = e.Reader.ReadUInt16();
            simulationTime = e.Reader.ReadSingle();
            value = e.Reader.ReadBoolean();
        }
    }

    #endregion

    public class ClientConnectedResponse : IDarkRiftSerializable
    {
        public float SimulationTime;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SimulationTime);
        }

        public void Deserialize(DeserializeEvent e)
        {
            SimulationTime = e.Reader.ReadSingle();
        }
    }

    public class SimulationTimeSync : IDarkRiftSerializable
    {
        public float T0;
        public float T1;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(T0);
            e.Writer.Write(T1);
        }

        public void Deserialize(DeserializeEvent e)
        {
            T0 = e.Reader.ReadSingle();
            T1 = e.Reader.ReadSingle();
        }
    }

    public class SpawnUnitRequest : IDarkRiftSerializable
    {
        public ushort PrefabIndex;
        public Vector2 Position;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(PrefabIndex);
            e.Writer.Write(Position.x);
            e.Writer.Write(Position.y);
        }

        public void Deserialize(DeserializeEvent e)
        {
            PrefabIndex = e.Reader.ReadUInt16();
            Position.x = e.Reader.ReadSingle();
            Position.y = e.Reader.ReadSingle();
        }
    }

    public class SpawnUnitPacket : IDarkRiftSerializable
    {
        public ushort ClientID;
        public ushort PrefabIndex;
        public Vector2 Position;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
            e.Writer.Write(PrefabIndex);
            e.Writer.Write(Position.x);
            e.Writer.Write(Position.y);
        }

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadUInt16();
            PrefabIndex = e.Reader.ReadUInt16();
            Position.x = e.Reader.ReadSingle();
            Position.y = e.Reader.ReadSingle();
        }
    }

    public class ClientDisconnected : IDarkRiftSerializable
    {
        public ushort ClientID;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ClientID);
        }

        public void Deserialize(DeserializeEvent e)
        {
            ClientID = e.Reader.ReadUInt16();
        }
    }

}