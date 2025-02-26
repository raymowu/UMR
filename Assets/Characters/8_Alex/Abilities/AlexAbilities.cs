
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class AlexAbilities : CharacterAbilities
{
    [Header("Owangatang Flail")]
    public float FLAIL_DURATION = 1f;
    public float FLAIL_RANGE = 2.5f;
    public float FLAIL_ANGLE = 130f;
    public float FLAIL_DAMAGE = 20f;
    public float FLAIL_TICK_INTERVAL = 0.5f;

    [Header("Ate too Much")]
    public bool ability2Active = false;
    public float ATE_TOO_MUCH_TICK_INTERVAL = 0.5f;
    public float ATE_TOO_MUCH_DURATION = 3f;

    //[Header("Ability 3")]

    //[Header("Ability 4")]


    protected override void Ability3Canvas()
    {

    }

    protected override void Ability4Canvas()
    {

    }

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1Cooldown, ref currentAbility1Cooldown,
            "CastOwangatangFlail", () =>
            {
                ability2Active = true;
                float startTime = Time.time;
                if (Time.time < startTime + FLAIL_DURATION)
                {
                    StartCoroutine(Ability1Interval());
                }
            });
    }

    private IEnumerator Ability1Interval()
    {
        foreach (GameObject player in GetAllPlayersInRangeAndWithinAngle(FLAIL_RANGE, FLAIL_ANGLE))
        {
            GameManager.Instance.DealDamage(gameObject, player, FLAIL_DAMAGE);
        }
        yield return new WaitForSeconds(FLAIL_TICK_INTERVAL);
    }

    protected override void Ability2Input()
    {

        if (ability2Active == true && Time.time > nextTickTime)
        {
            // this updates every tick
            StartCoroutine(Ability2Interval());
        }

        InputHelper(ability2Key, ref isAbility2Cooldown, ability2Cooldown, ref currentAbility2Cooldown,
            "CastAteTooMuch", () =>
            {
                StartCoroutine(Ability2ActiveInterval());
                playerMovement.StopMovement();
                GameManager.Instance.Stun(gameObject, ATE_TOO_MUCH_DURATION);
            });
    }

    private IEnumerator Ability2Interval()
    {
        nextTickTime = Time.time + ATE_TOO_MUCH_TICK_INTERVAL;
        GameManager.Instance.Heal(gameObject, 50f);
        yield return new WaitForSeconds(ATE_TOO_MUCH_TICK_INTERVAL);
    }

    private IEnumerator Ability2ActiveInterval()
    {
        yield return new WaitForSeconds(ATE_TOO_MUCH_DURATION);
        ability2Active = false;
    }


    protected override void Ability3Input()
    {

    }

    protected override void Ability4Input()
    {
        
    }
}