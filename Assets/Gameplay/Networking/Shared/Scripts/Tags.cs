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
        JumpingInput
    }

    public enum ServerTag
    {
        ClientConnected,
        ClientDisconnected,
        UnitSpawned,
    }

    #region Input

    public class FloatInputPacket : IDarkRiftSerializable
    {
        public double fixedTime;
        public float value;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(fixedTime);
            e.Writer.Write(value);
        }

        public void Deserialize(DeserializeEvent e)
        {
            fixedTime = e.Reader.ReadDouble();
            value = e.Reader.ReadSingle();
        }
    }

    public class BoolInputPacket : IDarkRiftSerializable
    {
        public double fixedTime;
        public bool value;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(fixedTime);
            e.Writer.Write(value);
        }

        public void Deserialize(DeserializeEvent e)
        {
            fixedTime = e.Reader.ReadDouble();
            value = e.Reader.ReadBoolean();
        }
    }

    #endregion

    public class ClientConnectedResponse : IDarkRiftSerializable
    {
        public double SimulationTime;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(SimulationTime);
        }

        public void Deserialize(DeserializeEvent e)
        {
            SimulationTime = e.Reader.ReadDouble();
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