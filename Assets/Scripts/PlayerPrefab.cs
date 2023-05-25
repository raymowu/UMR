using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerPrefab : NetworkBehaviour
{
    public float Health;
    public float AttackSpeed;
    public float MovementSpeed;
    public float Damage;

    public void UpdatePlayerStats(PlayerStats player)
    {
        if (player.CharacterId != -1)
        {
            //var character = characterDatabase.GetCharacterById(player.CharacterId);
            //characterIconImage.sprite = character.Icon;
            //characterIconImage.enabled = true;
            //characterNameText.text = character.DisplayName;
            Health = player.Health;
            AttackSpeed = player.AttackSpeed;
            MovementSpeed = player.MovementSpeed;
            Damage = player.Damage;
        }
        //else
        //{
            //characterIconImage.enabled = false;
        //}
    }
}
