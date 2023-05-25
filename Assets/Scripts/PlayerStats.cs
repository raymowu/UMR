using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public struct PlayerStats : INetworkSerializable, IEquatable<PlayerStats>
{
    public ulong ClientId;
    public int CharacterId;
    public float Health;
    public float AttackSpeed;
    public float MovementSpeed;
    public float Damage;


    // Constructor
    public PlayerStats(ulong clientId, int characterId = -1, float health = 0, float attackSpeed = 0, float movementSpeed = 0, float damage = 0)
    {
        ClientId = clientId;
        CharacterId = characterId;
        Health = health;
        AttackSpeed = attackSpeed;
        MovementSpeed = movementSpeed;
        Damage = damage;
    }

    // Serialize the network variables
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref AttackSpeed);
        serializer.SerializeValue(ref MovementSpeed);
        serializer.SerializeValue(ref Damage);
    }

    // Checks if values have changed and need to sync
    public bool Equals(PlayerStats other)
    {
        return ClientId == other.ClientId &&
            CharacterId == other.CharacterId &&
            Health == other.Health &&
            AttackSpeed == other.AttackSpeed &&
            MovementSpeed == other.MovementSpeed &&
            Damage == other.Damage;
    }
}
