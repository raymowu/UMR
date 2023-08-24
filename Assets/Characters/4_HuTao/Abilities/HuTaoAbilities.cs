
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class HuTaoAbilities : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    [SerializeField] private Canvas abilitiesCanvas;
    private PlayerMovement playerMovement;
    private Animator anim;
    private PlayerPrefab stats;

    [Header("Ability 1")]
    public Image abilityImage1;
    public TMP_Text abilityText1;
    public KeyCode ability1Key = KeyCode.Q;
    public float ability1Cooldown;
    public Canvas ability1Canvas;
    public Image ability1Indicator;
    public GameObject ability1DisableOverlay;
    public float DASH_SPEED = 20f;
    public float DASH_TIME = 0.15f;

    [Header("Ability 2")]
    public GameObject spiritOfTheAfterlifeParticles;
    public Image abilityImage2;
    public TMP_Text abilityText2;
    public KeyCode ability2Key = KeyCode.W;
    private float nextTickTime = 0f;
    public GameObject ability2DisableOverlay;
    public float ABILITY2ACTIVATIONCOST = 0.02f;
    public float ABILITY2TICKINTERVAL = 0.5f;
    public float ABILITY2RANGE = 1.5f;
    public bool ability2Active = false;

    [Header("Ability 3")]
    public Image abilityImage3;
    public TMP_Text abilityText3;
    public KeyCode ability3Key = KeyCode.E;
    public float ability3Cooldown;
    public float ability3Duration = 9f;
    public GameObject ability3DisableOverlay;
    public float ABILITY3ACTIVATIONCOST = 0.3f;
    public float ABILITY3ATTACKINCREASE = .063f;

    [Header("Ability 4")]
    [SerializeField] private GameObject ability4Particles;
    public Image abilityImage4;
    public TMP_Text abilityText4;
    public KeyCode ability4Key = KeyCode.R;
    public float ability4Cooldown;
    public Canvas ability4Canvas;
    public Image ability4Indicator;
    public GameObject ability4DisableOverlay;
    public float ABILITY4RANGE = 4f;
    public float ABILITY4DAMAGE = 4.50f;
    public float ABILITY4LOWHPDAMAGE = 5.50f;
    public float ABILITY4HPREGEN = 0.1f;
    public float ABILITY4LOWHPREGEN = 0.2f;

    private bool isAbility1Cooldown = false;
    private bool isAbility3Cooldown = false;
    private bool isAbility4Cooldown = false;

    private float currentAbility1Cooldown;
    private float currentAbility3Cooldown;
    private float currentAbility4Cooldown;

    private Vector3 position;
    private RaycastHit hit;
    private Ray ray;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        stats = GetComponent<PlayerPrefab>();

        // Shows UI
        if (IsOwner)
        {
            abilitiesCanvas.gameObject.SetActive(true);
            ability1Canvas.gameObject.SetActive(true);
            ability4Canvas.gameObject.SetActive(true);
        }

        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;
        abilityImage3.fillAmount = 0;
        abilityImage4.fillAmount = 0;

        abilityText1.text = "";
        abilityText2.text = "";
        abilityText3.text = "";
        abilityText4.text = "";

        ability1Indicator.enabled = false;
        ability4Indicator.enabled = false;

        ability1Canvas.enabled = false;
        ability4Canvas.enabled = false;
    }

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

        Ability1Input();
        Ability2Input();
        Ability3Input();
        Ability4Input();

        AbilityCooldown(ref currentAbility1Cooldown, ability1Cooldown, ref isAbility1Cooldown, abilityImage1, abilityText1);
        AbilityCooldown(ref currentAbility3Cooldown, ability3Cooldown, ref isAbility3Cooldown, abilityImage3, abilityText3);
        AbilityCooldown(ref currentAbility4Cooldown, ability4Cooldown, ref isAbility4Cooldown, abilityImage4, abilityText4);

        Ability1Canvas();
        Ability4Canvas();
    }

    private void Ability1Canvas()
    {
        if (ability1Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }
            Quaternion ab1Canvas = Quaternion.LookRotation(position - transform.position);
            ab1Canvas.eulerAngles = new Vector3(0, ab1Canvas.eulerAngles.y, ab1Canvas.eulerAngles.z);

            ability1Canvas.transform.rotation = Quaternion.Lerp(ab1Canvas, ability1Canvas.transform.rotation, 0);
        }
    }

    private void Ability4Canvas()
    {
        if (ability4Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }
            Quaternion ab4Canvas = Quaternion.LookRotation(position - transform.position);
            ab4Canvas.eulerAngles = new Vector3(0, ab4Canvas.eulerAngles.y, ab4Canvas.eulerAngles.z);

            ability4Canvas.transform.rotation = Quaternion.Lerp(ab4Canvas, ability4Canvas.transform.rotation, 0);
        }
    }

    private void Ability1Input()
    {
        if (Input.GetKeyDown(ability1Key) && !isAbility1Cooldown)
        {
            ability1Canvas.enabled = true;
            ability1Indicator.enabled = true;

            ability4Canvas.enabled = false;
            ability4Indicator.enabled = false;

        }
        if (ability1Canvas.enabled && Input.GetKeyUp(ability1Key)) 
        {
            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;

            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

            playerMovement.Rotate(hit.point);
            StartCoroutine(Dash());
        }
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

    private void Ability2Input()
    {
        if (ability2Active && Time.time > nextTickTime)
        {
            StartCoroutine(Ability2Interval());
        }

        if (Input.GetKeyDown(ability2Key))
        {
            abilityImage2.fillAmount = ability2Active ? 0 : 1;

            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

            ability4Canvas.enabled = false;
            ability4Indicator.enabled = false;

            ability2Active = !ability2Active;
            ToggleAbility2ParticlesServerRpc(ability2Active);
        }

        if (stats.Health - (ABILITY2ACTIVATIONCOST * stats.MaxHealth) <= 100f)
        {
            abilityImage2.fillAmount = 1;
            ability2Active = false;
            ToggleAbility2ParticlesServerRpc(ability2Active);
        }
    }
    private IEnumerator Ability2Interval()
    {
        nextTickTime = Time.time + ABILITY2TICKINTERVAL;

        GameManager.Instance.DealDamage(gameObject, gameObject, ABILITY2ACTIVATIONCOST * stats.MaxHealth);
        foreach (GameObject player in GameManager.Instance.playerPrefabs)
        {
            if (player == gameObject) { continue; }
            if (Vector3.Distance(transform.position, player.transform.position) <= ABILITY2RANGE)
            {
                GameManager.Instance.DealDamage(gameObject, player, stats.Damage);
            }
        }
        yield return new WaitForSeconds(ABILITY2TICKINTERVAL);
    }

    [ServerRpc]
    private void ToggleAbility2ParticlesServerRpc(bool active)
    {
        ToggleAbility2ParticlesClientRpc(active);
    }

    [ClientRpc]
    private void ToggleAbility2ParticlesClientRpc(bool active)
    {
        spiritOfTheAfterlifeParticles.gameObject.SetActive(active);
    }

    private void Ability3Input()
    {
        if (Input.GetKeyDown(ability3Key) && !isAbility3Cooldown)
        {
            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

            ability4Canvas.enabled = false;
            ability4Indicator.enabled = false;

            isAbility3Cooldown = true;
            currentAbility3Cooldown = ability3Cooldown;

            playerMovement.StopMovement();
            GameManager.Instance.Root(gameObject, 1.5f);

            GameManager.Instance.SummonStrengthParticles(gameObject);
            GameManager.Instance.SummonGlowingParticles(gameObject, ability3Duration);
            CastAbility3ServerRpc();
        }
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

    private void Ability4Input()
    {
        if (Input.GetKeyDown(ability4Key) && !isAbility4Cooldown)
        {
            ability4Canvas.enabled = true;
            ability4Indicator.enabled = true;

            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

        }
        if (ability4Canvas.enabled && Input.GetKeyUp(ability4Key))
        {
            isAbility4Cooldown = true;
            currentAbility4Cooldown = ability4Cooldown;

            ability4Canvas.enabled = false;
            ability4Indicator.enabled = false;

            playerMovement.StopMovement();

            CastAbility4ServerRpc();

            anim.SetTrigger("CastSpiritSoother");

            int numEnemiesHit = 0;
            foreach (GameObject player in GameManager.Instance.playerPrefabs)
            {
                if (Vector3.Distance(transform.position, player.transform.position) <= ABILITY4RANGE)
                {
                    if (player == gameObject) { continue;  }
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
        }
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
