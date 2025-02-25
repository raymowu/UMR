
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class DarthogAbilities : CharacterAbilities
{

    [Header("Smash")]
    public float SMASH_RANGE = 3f;
    public float SMASH_KNOCKUP_DURATION = 1f;

    [Header("Pounce")]
    public float POUNCE_DASH_SPEED = 20f;
    public float POUNCE_DASH_TIME = 0.2f;
    public float POUNCE_DASH_RANGE = 5f;

    [Header("Rock Hurl")]
    [SerializeField] private GameObject ability3Projectile;
    public float ROCK_HURL_STUN_DURATION = 1.5f;

    [Header("Beast Awakening")]
    [SerializeField] GameObject STRENGTH_BUFF_PARTICLES;
    public float BEAST_AWAKENING_BUFF_AMOUNT = 0.2f;
    public float BEAST_AWAKENING_BUFF_DURATION = 15f;

    protected override void Ability2Canvas()
    {
        PointAndClickCanvas(ability2IndicatorCanvas);
    }

    protected override void Ability3Canvas()
    {
        LinearProjectileCanvas(ability2IndicatorCanvas);
    }

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1Cooldown, ref currentAbility1Cooldown,
            "CastSmash", () =>
            {
                foreach (GameObject player in GetAllPlayersInRange(SMASH_RANGE))
                {
                    GameManager.Instance.Knockup(player, SMASH_KNOCKUP_DURATION);
                }
            });
    }
    protected override void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ability2Cooldown, ref currentAbility2Cooldown,
            "CastPounce", POUNCE_DASH_RANGE, () =>
            {
                playerMovement.Rotate(hit.point);
                StartCoroutine(Dash());
            }
            );
    }

    IEnumerator Dash()
    {
        float startTime = Time.time;
        while (Time.time < startTime + POUNCE_DASH_TIME)
        {
            GetComponent<CharacterController>().Move(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position) * 
                Vector3.forward * POUNCE_DASH_SPEED * Time.deltaTime);
            playerMovement.StopMovement();
            yield return null;
        }
    }

    protected override void Ability3Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3IndicatorCanvas, ability3Cooldown,
                    ref currentAbility3Cooldown, "CastRockHurl", () => {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            CastAbility3ServerRpc(hit.point, Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                        }
                    });
    }

    [ServerRpc]
    private void CastAbility3ServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(ability3Projectile, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveRockHurl>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability4Input()
    {
        InputHelper(ability4Key, ref isAbility4Cooldown, ability4Cooldown, ref currentAbility4Cooldown, 
           "CastBeastAwakening", () => {
               GameManager.Instance.SummonStrengthParticles(gameObject);
               GameManager.Instance.SummonGlowingParticles(gameObject, BEAST_AWAKENING_BUFF_DURATION);
               playerMovement.StopMovement();
               GameManager.Instance.Root(gameObject, 3f);
               GetComponent<OwnerNetworkAnimator>().SetTrigger("CastBeastAwakening");

               StartCoroutine(BeastAwakening(stats.AttackSpeed, stats.Damage, stats.MaxHealth, BEAST_AWAKENING_BUFF_DURATION));
               GameManager.Instance.Speed(gameObject, stats.MovementSpeed + (stats.MovementSpeed * BEAST_AWAKENING_BUFF_AMOUNT), BEAST_AWAKENING_BUFF_DURATION);
               GameManager.Instance.IncreaseAttackSpeed(gameObject, stats.AttackSpeed * BEAST_AWAKENING_BUFF_AMOUNT);
               GameManager.Instance.IncreaseDamage(gameObject, stats.Damage * BEAST_AWAKENING_BUFF_AMOUNT);
               GameManager.Instance.IncreaseMaxHealth(gameObject, stats.MaxHealth * BEAST_AWAKENING_BUFF_AMOUNT);
               GameManager.Instance.Heal(gameObject, stats.Health * BEAST_AWAKENING_BUFF_AMOUNT);
               }
           );
    }

    IEnumerator BeastAwakening(float originalAttackSpeed, float originalDamage, float originalMaxHealth, float duration)
    {
        yield return new WaitForSeconds(duration);
        GameManager.Instance.SetAttackSpeed(gameObject, originalAttackSpeed);
        GameManager.Instance.SetDamage(gameObject, originalDamage);
        GameManager.Instance.SetMaxHealth(gameObject, originalMaxHealth);
    }
}
