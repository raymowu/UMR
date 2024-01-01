using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public struct MobStats : INetworkSerializable, IEquatable<MobStats>
{
    // ID is UNIQUE TO THAT INSTANCE OF MOB IN NETWORK PREFABS, not that mob
    public ulong Id;
    public int MobId;
    public float MaxHealth;
    public float Health;
    public float AttackSpeed;
    public float MovementSpeed;
    public float CurrentMovementSpeed;
    public float Damage;
    public bool IsSilenced;
    public bool IsDisarmed;
    public bool IsDead;


    // Constructor
    public MobStats(ulong id, int mobId = -1, float maxHealth = 0, float health = 0, float attackSpeed = 0,
        float movementSpeed = 0, float currentMovementSpeed = 0, float damage = 0, bool isSilenced = false, bool isDisarmed = false, bool isDead = false)
    {
        Id = id;
        MobId = mobId;
        MaxHealth = maxHealth;
        Health = health;
        AttackSpeed = attackSpeed;
        MovementSpeed = movementSpeed;
        CurrentMovementSpeed = currentMovementSpeed;
        Damage = damage;
        IsSilenced = isSilenced;
        IsDisarmed = isDisarmed;
        IsDead = isDead;
    }

    // Serialize the network variables
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Id);
        serializer.SerializeValue(ref MobId);
        serializer.SerializeValue(ref MaxHealth);
        serializer.SerializeValue(ref Health);
        serializer.SerializeValue(ref AttackSpeed);
        serializer.SerializeValue(ref MovementSpeed);
        serializer.SerializeValue(ref CurrentMovementSpeed);
        serializer.SerializeValue(ref Damage);
        serializer.SerializeValue(ref IsSilenced);
        serializer.SerializeValue(ref IsDisarmed);
        serializer.SerializeValue(ref IsDead);
    }

    // Checks if values have changed and need to sync
    public bool Equals(MobStats other)
    {
        return Id == other.Id &&
            MobId == other.MobId &&
            MaxHealth == other.MaxHealth &&
            Health == other.Health &&
            AttackSpeed == other.AttackSpeed &&
            MovementSpeed == other.MovementSpeed &&
            CurrentMovementSpeed == other.CurrentMovementSpeed &&
            Damage == other.Damage &&
            IsSilenced == other.IsSilenced &&
            IsDisarmed == other.IsDisarmed &&
            IsDead == other.IsDead;
    }
}
