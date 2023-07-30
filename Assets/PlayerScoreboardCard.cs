using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreboardCard : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text statsText;
   //TODO: items

    public void UpdateDisplay(PlayerStats player)
    {
        if (player.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(player.CharacterId);
            characterIconImage.sprite = character.Icon;
            characterIconImage.enabled = true;

        }
        else
        {
            characterIconImage.enabled = false;
        }

        statsText.text = player.Kills + "/" + player.Deaths;

        visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }
}
