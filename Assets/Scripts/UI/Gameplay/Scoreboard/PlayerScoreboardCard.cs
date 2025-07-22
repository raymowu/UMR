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
    [SerializeField] private TMP_Text kdText;
    [SerializeField] private TMP_Text csText;
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

        kdText.text = player.Kills + "/" + player.Deaths;
        csText.text = player.MobScore + "";

        visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }
}
