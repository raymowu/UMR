using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class MobPrefab : NetworkBehaviour
{
    [SerializeField] private HealthUI healthUI;
    [Header("Mob Id")]
    public int MobId;
    [Header("Base Stats")]
    public float MaxHealth;
    public float Health;
    public float AttackSpeed;
    public float MovementSpeed;
    public float CurrentMovementSpeed;
    public float Damage;
    public bool IsSilenced;
    public bool IsDisarmed;
    public bool IsDead;

    void Start()
    {
        healthUI = GetComponent<HealthUI>();
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if (Input.GetKeyDown(KeyCode.V))
        {
            GameManager.Instance.DealDamage(gameObject, gameObject, 9999);
        }
    }

    public void UpdateMobStats(MobStats mob)
    {
        if (mob.MobId != -1)
        {
            MaxHealth = mob.MaxHealth;
            Health = mob.Health;
            AttackSpeed = mob.AttackSpeed;
            MovementSpeed = mob.MovementSpeed;
            CurrentMovementSpeed = mob.CurrentMovementSpeed;
            Damage = mob.Damage;
            IsSilenced = mob.IsSilenced;
            IsDisarmed = mob.IsDisarmed;
            IsDead = mob.IsDead;

            healthUI.Update3DSlider(mob.MaxHealth, Health);
        }
    }
}
