using DarkRift;
using UnityEngine;

namespace Network.Shared
{

    public enum Tag
    {
        // Connection
        ClientConnectedResponse,
        ClientDisconnected,

        // Units
        SpawnUnitRequest,
        SpawnUnitResponse,
        
        // Input
        MovementInput
    }

    #region Input

    public class FloatInputPacket : IDarkRiftSerializable
    {
        public float fixedTime;
        public float value;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(fixedTime);
            e.Writer.Write(value);
        }

        public void Deserialize(DeserializeEvent e)
        {
            fixedTime = e.Reader.ReadSingle();
            value = e.Reader.ReadSingle();
        }
    }

    #endregion

    public class ClientConnectedResponse : IDarkRiftSerializable
    {
        public uint CurrentTick;

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(CurrentTick);
        }

        public void Deserialize(DeserializeEvent e)
        {
            CurrentTick = e.Reader.ReadUInt32();
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

    public class SpawnUnitResponse : IDarkRiftSerializable
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