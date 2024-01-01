using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Mob", menuName = "Mobs/Mob")]
public class Mob : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private NetworkObject gameplayPrefab;
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float currentMovementSpeed;
    [SerializeField] private float damage;


    // Get private stuff publicly
    public int Id => id;
    public string DisplayName => displayName;
    public NetworkObject GameplayPrefab => gameplayPrefab;
    public float MaxHealth => maxHealth;
    public float Health => health;
    public float AttackSpeed => attackSpeed;
    public float MovementSpeed => movementSpeed;
    public float CurrentMovementSpeed => currentMovementSpeed;
    public float Damage => damage;
}
