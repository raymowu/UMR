using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public struct PlayerStats : INetworkSerializable, IEquatable<PlayerStats>
{
    public ulong ClientId;
    public int CharacterId;
    public float MaxHealth;
    public float Health;
    public float AttackSpeed;
    public float MovementSpeed;
    public float CurrentMovementSpeed;
    public float Damage;
    public bool IsSilenced;
    public bool IsDisarmed;
    public int Kills;
    public int Deaths;


    // Constructor
    public PlayerStats(ulong clientId, int characterId = -1, float maxHealth = 0, float health = 0, float attackSpeed = 0,
        float movementSpeed = 0, float currentMovementSpeed = 0, float damage = 0, bool isSilenced = false, bool isDisarmed = false, int kills = 0, int deaths = 0)
    {
        ClientId = clientId;
        CharacterId = characterId;
        MaxHealth = maxHealth;
        Health = health;
        AttackSpeed = attackSpeed;
        MovementSpeed = movementSpeed;
        CurrentMovementSpeed = currentMovementSpeed;
        Damage = damage;
        IsSilenced = isSilenced;
        IsDisarmed = isDisarmed;
        Kills = kills;
        Deaths = deaths;
    }

    // Serialize the network variables
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref MaxHealth);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref AttackSpeed);
        serializer.SerializeValue(ref MovementSpeed);
        serializer.SerializeValue(ref CurrentMovementSpeed);
        serializer.SerializeValue(ref Damage);
        serializer.SerializeValue(ref IsSilenced);
        serializer.SerializeValue(ref IsDisarmed);
        serializer.SerializeValue(ref Kills);
        serializer.SerializeValue(ref Deaths);
    }

    // Checks if values have changed and need to sync
    public bool Equals(PlayerStats other)
    {
        return ClientId == other.ClientId &&
            CharacterId == other.CharacterId &&
            MaxHealth == other.MaxHealth &&
            Health == other.Health &&
            AttackSpeed == other.AttackSpeed &&
            MovementSpeed == other.MovementSpeed &&
            CurrentMovementSpeed == other.CurrentMovementSpeed &&
            Damage == other.Damage &&
            IsSilenced == other.IsSilenced &&
            IsDisarmed == other.IsDisarmed &&
            Kills == other.Kills &&
            Deaths == other.Deaths;
    }
}
