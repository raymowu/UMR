using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName ="New Character", menuName = "Characters/Character")]
public class Character : ScriptableObject
{
    [SerializeField] private int id = -1;
    [SerializeField] private string displayName = "New Display Name";
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject introPrefab;
    [SerializeField] private NetworkObject gameplayPrefab;
    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float damage;


    // Get private stuff publicly
    public int Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public GameObject IntroPrefab => introPrefab;
    public NetworkObject GameplayPrefab => gameplayPrefab;
    public float MaxHealth => maxHealth;
    public float Health => health;
    public float AttackSpeed => attackSpeed;
    public float MovementSpeed => movementSpeed;
    public float Damage => damage;

}
