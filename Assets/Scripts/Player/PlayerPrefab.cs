using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerPrefab : NetworkBehaviour
{
    [SerializeField] private HealthUI healthUI;
    [Header("Base Stats")]
    public float MaxHealth;
    public float Health;
    public float AttackSpeed;
    public float MovementSpeed;
    public float Damage;
    public bool IsSilenced;
    public bool IsDisarmed;

    void Start()
    {
        healthUI = GetComponent<HealthUI>();
    }
    //TESTING ONLY
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            GameManager.Instance.DealDamage(gameObject, Damage);
        }
    }

    public void UpdatePlayerStats(PlayerStats player)
    {
        if (player.CharacterId != -1)
        {
            //var character = characterDatabase.GetCharacterById(player.CharacterId);
            //characterIconImage.sprite = character.Icon;
            //characterIconImage.enabled = true;
            //characterNameText.text = character.DisplayName;
            MaxHealth = player.MaxHealth;
            Health = player.Health;
            AttackSpeed = player.AttackSpeed;
            MovementSpeed = player.MovementSpeed;
            Damage = player.Damage;
            IsSilenced = player.IsSilenced;
            IsDisarmed = player.IsDisarmed;

            healthUI.Update2DSlider(player.MaxHealth, Health);
            healthUI.Update3DSlider(player.MaxHealth, Health);
        }
        //else
        //{
        //characterIconImage.enabled = false;
        //}
    }
}
