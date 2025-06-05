using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerPrefab : NetworkBehaviour
{
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private GoldUI goldUI;
    [Header("Base Stats")]
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
    public bool IsDead;
    public int Gold;

    void Start()
    {
        healthUI = GetComponent<HealthUI>();
        goldUI = GetComponent<GoldUI>();
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        /*if (Input.GetKeyDown(KeyCode.V))
        {
            GameManager.Instance.DealDamage(gameObject, gameObject, 9999);
        }*/
    }

    public void UpdatePlayerStats(PlayerStats player)
    {
        if (player.CharacterId != -1)
        {
            MaxHealth = player.MaxHealth;
            Health = player.Health;
            AttackSpeed = player.AttackSpeed;
            MovementSpeed = player.MovementSpeed;
            CurrentMovementSpeed = player.CurrentMovementSpeed;
            Damage = player.Damage;
            IsSilenced = player.IsSilenced;
            IsDisarmed = player.IsDisarmed;
            Kills = player.Kills;
            Deaths = player.Deaths;
            IsDead = player.IsDead;
            Gold = player.Gold;

            healthUI.Update2DSlider(player.MaxHealth, Health);
            healthUI.Update3DSlider(player.MaxHealth, Health);
            goldUI.UpdateGold(player.Gold);
        }
    }
}
