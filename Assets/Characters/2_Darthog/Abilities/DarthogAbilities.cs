
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class DarthogAbilities : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    [SerializeField] private Canvas abilitiesCanvas;
    private PlayerMovement playerMovement;
    private PlayerPrefab stats;

    [Header("Ability 1")]
    public float SMASH_RANGE = 3f;
    public float SMASH_KNOCKUP_DURATION = 1f;
    public Image abilityImage1;
    public TMP_Text abilityText1;
    public KeyCode ability1Key = KeyCode.Q;
    public float ability1Cooldown;
    public GameObject ability1DisableOverlay;

    [Header("Ability 2")]
    public float POUNCE_DASH_SPEED = 20f;
    public float POUNCE_DASH_TIME = 0.2f;
    public float POUNCE_DASH_RANGE = 5f;
    public Image abilityImage2;
    public TMP_Text abilityText2;
    public KeyCode ability2Key = KeyCode.W;
    public float ability2Cooldown;
    public Canvas ability2Canvas;
    public Image ability2Indicator;
    public GameObject ability2DisableOverlay;

    [Header("Ability 3")]
    public float ROCK_HURL_STUN_DURATION = 1.5f;
    public Image abilityImage3;
    public TMP_Text abilityText3;
    public KeyCode ability3Key = KeyCode.E;
    public float ability3Cooldown;

    [SerializeField] private GameObject ability3Projectile;

    public Canvas ability3Canvas;
    public Image ability3Indicator;
    public GameObject ability3DisableOverlay;

    [Header("Ability 4")]
    [SerializeField] GameObject STRENGTH_BUFF_PARTICLES;
    [SerializeField] GameObject BEAST_AWAKENING_PARTICLES;
    public float BEAST_AWAKENING_BUFF_AMOUNT = 0.2f;
    public float BEAST_AWAKENING_BUFF_DURATION = 15f;
    public Image abilityImage4;
    public TMP_Text abilityText4;
    public KeyCode ability4Key = KeyCode.R;
    public float ability4Cooldown;
    public GameObject ability4DisableOverlay;

    private bool isAbility1Cooldown = false;
    private bool isAbility2Cooldown = false;
    private bool isAbility3Cooldown = false;
    private bool isAbility4Cooldown = false;

    private float currentAbility1Cooldown;
    private float currentAbility2Cooldown;
    private float currentAbility3Cooldown;
    private float currentAbility4Cooldown;

    private Vector3 position;
    private RaycastHit hit;
    private Ray ray;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerPrefab>();

        // Shows UI
        if (IsOwner)
        {
            abilitiesCanvas.gameObject.SetActive(true);
            ability2Canvas.gameObject.SetActive(true);
            ability3Canvas.gameObject.SetActive(true);
        }

        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;
        abilityImage3.fillAmount = 0;
        abilityImage4.fillAmount = 0;

        abilityText1.text = "";
        abilityText2.text = "";
        abilityText3.text = "";
        abilityText4.text = "";

        ability2Indicator.enabled = false;
        ability3Indicator.enabled = false;

        ability2Canvas.enabled = false;
        ability3Canvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        if (stats.IsSilenced)
        {
            ability1DisableOverlay.SetActive(true);
            ability2DisableOverlay.SetActive(true);
            ability3DisableOverlay.SetActive(true);
            ability4DisableOverlay.SetActive(true);
            return;
        }
        ability1DisableOverlay.SetActive(false);
        ability2DisableOverlay.SetActive(false);
        ability3DisableOverlay.SetActive(false);
        ability4DisableOverlay.SetActive(false);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);


        // TODO: Cast ability functionality
        Ability1Input();
        Ability2Input();
        Ability3Input();
        Ability4Input();

        AbilityCooldown(ref currentAbility1Cooldown, ability1Cooldown, ref isAbility1Cooldown, abilityImage1, abilityText1);
        AbilityCooldown(ref currentAbility2Cooldown, ability2Cooldown, ref isAbility2Cooldown, abilityImage2, abilityText2);
        AbilityCooldown(ref currentAbility3Cooldown, ability3Cooldown, ref isAbility3Cooldown, abilityImage3, abilityText3);
        AbilityCooldown(ref currentAbility4Cooldown, ability4Cooldown, ref isAbility4Cooldown, abilityImage4, abilityText4);

        Ability2Canvas();
        Ability3Canvas();
    }

    private void Ability2Canvas()
    {
        if (ability2Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }
            Quaternion ab2Canvas = Quaternion.LookRotation(position - transform.position);
            ab2Canvas.eulerAngles = new Vector3(0, ab2Canvas.eulerAngles.y, ab2Canvas.eulerAngles.z);

            ability2Canvas.transform.rotation = Quaternion.Lerp(ab2Canvas, ability2Canvas.transform.rotation, 0);
        }
    }

    private void Ability3Canvas()
    {
        if (ability3Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }
            Quaternion ab3Canvas = Quaternion.LookRotation(position - transform.position);
            ab3Canvas.eulerAngles = new Vector3(0, ab3Canvas.eulerAngles.y, ab3Canvas.eulerAngles.z);

            ability3Canvas.transform.rotation = Quaternion.Lerp(ab3Canvas, ability3Canvas.transform.rotation, 0);
        }
    }

    private void Ability1Input()
    {
        if (Input.GetKeyDown(ability1Key) && !isAbility1Cooldown)
        {
            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;
            abilityImage2.fillAmount = 0;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;

            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;

            foreach (GameObject player in GameManager.Instance.playerPrefabs)
            {
                if (Vector3.Distance(transform.position, player.transform.position) <= SMASH_RANGE)
                {
                    if (player == gameObject) { continue; }
                    GameManager.Instance.Knockup(player, SMASH_KNOCKUP_DURATION);
                }
            }
        }
    }

    private void Ability2Input()
    {
        if (Input.GetKeyDown(ability2Key) && !isAbility2Cooldown)
        {
            ability2Canvas.enabled = true;
            ability2Indicator.enabled = true;
            abilityImage2.fillAmount = 1;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
        }
        GameObject targetEnemy = GetComponent<PlayerMovement>().targetEnemy;
        if (!isAbility2Cooldown && ability2Canvas.enabled && targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.transform.position) <= POUNCE_DASH_RANGE)
        {
            isAbility2Cooldown = true;
            currentAbility2Cooldown = ability2Cooldown;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;

            playerMovement.Rotate(hit.point);
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        float startTime = Time.time;
        while (Time.time < startTime + POUNCE_DASH_TIME)
        {
            GetComponent<CharacterController>().Move(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position) * Vector3.forward * POUNCE_DASH_SPEED * Time.deltaTime);
            playerMovement.StopMovement();
            yield return null;
        }
    }

    private void Ability3Input()
    {
        if (Input.GetKeyDown(ability3Key) && !isAbility3Cooldown)
        {
            ability3Canvas.enabled = true;
            ability3Indicator.enabled = true;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;
            abilityImage2.fillAmount = 0;
        }
        if (ability3Canvas.enabled && Input.GetKeyUp(ability3Key))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                playerMovement.StopMovement();
                playerMovement.Rotate(hit.point);
                CastAbility3ServerRpc(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
            }

            isAbility3Cooldown = true;
            currentAbility3Cooldown = ability3Cooldown;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;

        }
    }

    [ServerRpc]
    private void CastAbility3ServerRpc(Quaternion rot)
    {
        GameObject go = Instantiate(ability3Projectile, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveRockHurl>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    private void Ability4Input()
    {
        if (Input.GetKeyDown(ability4Key) && !isAbility4Cooldown)
        {
            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;
            abilityImage2.fillAmount = 0;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;

            isAbility4Cooldown = true;
            currentAbility4Cooldown = ability4Cooldown;

            SummonGlowingParticlesServerRpc();
            playerMovement.StopMovement();
            GameManager.Instance.Root(gameObject, 2.167f);
            GetComponent<OwnerNetworkAnimator>().SetTrigger("CastBeastAwakening");

            StartCoroutine(BeastAwakening(stats.AttackSpeed, stats.Damage, stats.MaxHealth, BEAST_AWAKENING_BUFF_DURATION));
            GameManager.Instance.Speed(gameObject, stats.MovementSpeed + (stats.MovementSpeed * BEAST_AWAKENING_BUFF_AMOUNT), BEAST_AWAKENING_BUFF_DURATION);
            GameManager.Instance.IncreaseAttackSpeed(gameObject, stats.AttackSpeed * BEAST_AWAKENING_BUFF_AMOUNT);
            GameManager.Instance.IncreaseDamage(gameObject, stats.Damage * BEAST_AWAKENING_BUFF_AMOUNT);
            GameManager.Instance.IncreaseMaxHealth(gameObject, stats.MaxHealth * BEAST_AWAKENING_BUFF_AMOUNT);
            GameManager.Instance.Heal(gameObject, stats.Health * BEAST_AWAKENING_BUFF_AMOUNT);
        }
    }


    [ServerRpc]
    private void SummonGlowingParticlesServerRpc()
    {
        SummonGlowingParticlesClientRpc();
    }

    [ClientRpc]
    private void SummonGlowingParticlesClientRpc()
    {
        // Constant glowing particles
        BEAST_AWAKENING_PARTICLES.SetActive(true);

        // Strength buff particles (destroy is handled automatically by particlesystem)
        GameObject go = Instantiate(STRENGTH_BUFF_PARTICLES, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation, gameObject.transform);
        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        ps.Stop();
        var main = ps.main;
        main.duration = BEAST_AWAKENING_BUFF_DURATION;
        ps.Play();
    }

    [ServerRpc]
    private void DestroyGlowingParticlesServerRpc()
    {
        DestroyGlowingParticlesClientRpc();
    }

    [ClientRpc]
    private void DestroyGlowingParticlesClientRpc()
    {
        BEAST_AWAKENING_PARTICLES.SetActive(false);
    }

    IEnumerator BeastAwakening(float originalAttackSpeed, float originalDamage, float originalMaxHealth, float duration)
    {
        yield return new WaitForSeconds(duration);
        DestroyGlowingParticlesServerRpc();
        GameManager.Instance.SetAttackSpeed(gameObject, originalAttackSpeed);
        GameManager.Instance.SetDamage(gameObject, originalDamage);
        GameManager.Instance.SetMaxHealth(gameObject, originalMaxHealth);
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
