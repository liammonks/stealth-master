using DarkRift;
using System;
using UnityEngine;

public enum Tag
{
    ClientConnected,
    ClientDisconnected,
    SyncTime,
    LatencyUpdate,
    LoadEnvironment,
    GameStart,
    ClientMoveInput
}

public class ClientConnected : IDarkRiftSerializable
{
    public ushort clientID;

    public void Deserialize(DeserializeEvent e)
    {
        clientID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(clientID);
    }
}

public class ClientDisconnected : IDarkRiftSerializable
{
    public ushort clientID;

    public void Deserialize(DeserializeEvent e)
    {
        clientID = e.Reader.ReadUInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(clientID);
    }
}


public class SyncTime : IDarkRiftSerializable
{
    public float clientRequestTime;
    public float serverReceiveTime;

    public void Deserialize(DeserializeEvent e)
    {
        clientRequestTime = e.Reader.ReadSingle();
        serverReceiveTime = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(clientRequestTime);
        e.Writer.Write(serverReceiveTime);
    }
}

public class LatencyUpdate : IDarkRiftSerializable
{
    public ushort clientID;
    public float latency;

    public void Deserialize(DeserializeEvent e)
    {
        clientID = e.Reader.ReadUInt16();
        latency = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(clientID);
        e.Writer.Write(latency);
    }
}

public class LoadEnvironment : IDarkRiftSerializable
{
    public int seed;

    public void Deserialize(DeserializeEvent e)
    {
        seed = e.Reader.ReadInt16();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(seed);
    }
}

public class GameStart : IDarkRiftSerializable
{
    public float startTime;

    public void Deserialize(DeserializeEvent e)
    {
        startTime = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(startTime);
    }
}

public class ClientMoveInput : IDarkRiftSerializable
{
    public float audioTime;
    public Vector2Int moveInput;

    public void Deserialize(DeserializeEvent e)
    {
        audioTime = e.Reader.ReadSingle();
        moveInput.x = e.Reader.ReadInt32();
        moveInput.y = e.Reader.ReadInt32();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(audioTime);
        e.Writer.Write(moveInput.x);
        e.Writer.Write(moveInput.y);
    }
}