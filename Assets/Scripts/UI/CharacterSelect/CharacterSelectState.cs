using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool IsLockedIn;

    // Constructor
    public CharacterSelectState(ulong clientId, int characterId = -1, bool isLockedIn = false)
    {
        ClientId = clientId;
        CharacterId = characterId;
        IsLockedIn = isLockedIn;
    }

    // Serialize the network variables
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsLockedIn);
    }

    // Checks if values have changed and need to sync
    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId && 
            CharacterId == other.CharacterId &&
            IsLockedIn == other.IsLockedIn;
    }
}
