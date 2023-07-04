using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public struct MovementSpeedBuffDebuff : INetworkSerializable, IEquatable<MovementSpeedBuffDebuff>
{
    public ulong ClientId;
    public float Amount;
    public float Duration;

    // Constructor
    public MovementSpeedBuffDebuff(ulong clientId, float amount = 0, float duration = 0)
    {
        ClientId = clientId;
        Amount = amount;
        Duration = duration;
    }

    // Serialize the network variables
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Amount);
        serializer.SerializeValue(ref Duration);
    }

    // Checks if values have changed and need to sync
    public bool Equals(MovementSpeedBuffDebuff other)
    {
        return ClientId == other.ClientId &&
            Amount == other.Amount &&
            Duration == other.Duration;
    }
}
