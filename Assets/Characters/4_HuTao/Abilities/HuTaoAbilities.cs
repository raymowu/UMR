
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class HuTaoAbilities : CharacterAbilities
{
    [Header("Secret Spear of Wangsheng")]
    public float DASH_SPEED = 20f;
    public float DASH_TIME = 0.15f;

    [Header("Spirit of Afterlife")]
    public GameObject spiritOfTheAfterlifeParticles;
    public float ABILITY2ACTIVATIONCOST = 0.02f;
    public float ABILITY2TICKINTERVAL = 0.5f;
    public float ABILITY2RANGE = 1.5f;

    [Header("Guide to Afterlife")]
    public float ability3Duration = 9f;
    public float ABILITY3ACTIVATIONCOST = 0.3f;
    public float ABILITY3ATTACKINCREASE = .063f;

    [Header("Spirit Soother")]
    [SerializeField] private GameObject ability4Particles;
    public float ABILITY4RANGE = 4f;
    public float ABILITY4DAMAGE = 4.50f;
    public float ABILITY4LOWHPDAMAGE = 5.50f;
    public float ABILITY4HPREGEN = 0.1f;
    public float ABILITY4LOWHPREGEN = 0.2f;

    protected override void Ability1Canvas()
    {
        LinearProjectileCanvas(ability1IndicatorCanvas);
    }

    protected override void Ability4Canvas()
    {
        LinearProjectileCanvas(ability4IndicatorCanvas);
    }

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ability1Cooldown,
            ref currentAbility1Cooldown, "CastSecretSpearOfWangsheng", () =>
            {
                playerMovement.Rotate(hit.point);
                StartCoroutine(Dash());
            }
        );
    }

    IEnumerator Dash()
    {
        float startTime = Time.time;
        while(Time.time < startTime + DASH_TIME)
        {
            GetComponent<CharacterController>().Move(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - 
                transform.position) * Vector3.forward * DASH_SPEED * Time.deltaTime);
            playerMovement.StopMovement();
            yield return null;
        }
    }

    protected override void Ability2Input()
    {
        ToggleInputHelper(ability2Key, ABILITY2TICKINTERVAL, () =>
        {
            GameManager.Instance.DealDamage(gameObject, gameObject, ABILITY2ACTIVATIONCOST * stats.MaxHealth);
            foreach (GameObject player in GetAllPlayersInRange(ABILITY2RANGE))
            {
                GameManager.Instance.DealDamage(gameObject, player, stats.Damage);
            }
        }, () => {
            ToggleSpiritOfAfterlifeParticlesServerRpc();
        });
        if (stats.Health - (ABILITY2ACTIVATIONCOST * stats.MaxHealth) <= 100f)
        {
            abilityImage2.fillAmount = 1;
            toggleActive = false;
            ToggleSpiritOfAfterlifeParticlesServerRpc();
        }
    }

    [ServerRpc]
    private void ToggleSpiritOfAfterlifeParticlesServerRpc()
    {
        ToggleSpiritofAfterlifeParticlesClientRpc();
    }

    [ClientRpc]
    private void ToggleSpiritofAfterlifeParticlesClientRpc()
    {
        spiritOfTheAfterlifeParticles.gameObject.SetActive(toggleActive);
    }

    protected override void Ability3Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3Cooldown, ref currentAbility3Cooldown, "CastGuideToAfterlife", () =>
        {
            GameManager.Instance.Root(gameObject, 0.5f);
            GameManager.Instance.SummonStrengthParticles(gameObject);
            GameManager.Instance.SummonGlowingParticles(gameObject, ability3Duration);
            CastAbility3ServerRpc();
        });
    }

    [ServerRpc]
    private void CastAbility3ServerRpc()
    {
        // ability cost: 30% of CURRENT HP
        GameManager.Instance.DealDamage(gameObject, gameObject, stats.Health * ABILITY3ACTIVATIONCOST);
        // atk increase (% max hp)
        GameManager.Instance.IncreaseDamage(gameObject, stats.MaxHealth * ABILITY3ATTACKINCREASE);
        StartCoroutine(DestroyGuideToAfterlife());
    }
    IEnumerator DestroyGuideToAfterlife()
    {
        yield return new WaitForSeconds(ability3Duration);
        DestroyGuideToAfterlifeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyGuideToAfterlifeServerRpc()
    {
        GameManager.Instance.DecreaseDamage(gameObject, stats.MaxHealth * ABILITY3ATTACKINCREASE);
    }

    protected override void Ability4Input()
    {
        InputHelper(ability4Key, ref isAbility4Cooldown, ability4IndicatorCanvas, ability4Cooldown, ref currentAbility4Cooldown, "CastSpiritSoother", () =>
        {
            playerMovement.StopMovement();
            CastAbility4ServerRpc();
            int numEnemiesHit = 0;
            foreach (GameObject player in GameManager.Instance.playerPrefabs)
            {
                if (Vector3.Distance(transform.position, player.transform.position) <= ABILITY4RANGE)
                {
                    if (player == gameObject) { continue; }
                    numEnemiesHit++;
                    if (stats.Health / stats.MaxHealth <= 0.5f)
                    {
                        GameManager.Instance.DealDamage(gameObject, player, stats.Damage * ABILITY4LOWHPDAMAGE);
                    }
                    else
                    {
                        GameManager.Instance.DealDamage(gameObject, player, stats.Damage * ABILITY4DAMAGE);
                    }
                }
            }
            if (stats.Health / stats.MaxHealth <= 0.5)
            {
                GameManager.Instance.Heal(gameObject, numEnemiesHit * ABILITY4LOWHPREGEN * stats.MaxHealth);
            }
            else
            {
                GameManager.Instance.Heal(gameObject, numEnemiesHit * ABILITY4HPREGEN * stats.MaxHealth);
            }
        });
        }

    [ServerRpc]
    private void CastAbility4ServerRpc()
    {
        GameObject go = Instantiate(ability4Particles, shootTransform.position, Quaternion.Euler(90, 0, 0));
        go.GetComponent<NetworkObject>().Spawn();
    }
    private void AbilityCooldown(ref float currentCooldown, float maxCooldown, ref bool isCooldown, Image skillImage, TMP_Text skillText)
    {
        if (isCooldown)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown <= 0f)
            {
                isCooldown = false;
                currentCooldown = 0f;

                if (skillImage != null)
                {
                    skillImage.fillAmount = 0f;
                }
                if (skillText != null)
                {
                    skillText.text = "";
                }
            }
            else
            {
                if (skillImage != null)
                {
                    skillImage.fillAmount = currentCooldown / maxCooldown;
                }
                if (skillText != null)
                {
                    skillText.text = Mathf.Ceil(currentCooldown).ToString();
                }
            }
        }
    }
}
