using DarkRift;
using UnityEngine;

public enum Tags
{
    ClientConnectedResponse,
    SpawnUnitRequest,
    SpawnUnitResponse,
    ClientDisconnected,
    InputPacket
}

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

public class InputPacket : IDarkRiftSerializable
{
    public float Movement;
    public bool Running;

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(Movement);
        e.Writer.Write(Running);
    }

    public void Deserialize(DeserializeEvent e)
    {
        Movement = e.Reader.ReadSingle();
        Running = e.Reader.ReadBoolean();
    }
}