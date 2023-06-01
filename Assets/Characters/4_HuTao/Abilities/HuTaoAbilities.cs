
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

    public float ABILITY1DASHSPEED;
    public float ABILITY1DASHTIME;

    [Header("Ability 2")]
    public Image abilityImage2;
    public TMP_Text abilityText2;
    public KeyCode ability2Key = KeyCode.W;

    [SerializeField] private GameObject ability2Particles;

    public bool ability2Active = false;
    private float nextTickTime = 0f;
    private const float ABILITY2ACTIVATIONCOST = 0.02f;
    private const float ABILITY2TICKINTERVAL = 0.5f;
    private const float ABILITY2RANGE = 3.5f;

    [Header("Ability 3")]
    public Image abilityImage3;
    public TMP_Text abilityText3;
    public KeyCode ability3Key = KeyCode.E;
    public float ability3Cooldown;

    public float ability3Duration = 9f;
    private const float ABILITY3ACTIVATIONCOST = 0.3f;
    private const float ABILITY3ATTACKINCREASE = .063f;

    [Header("Ability 4")]
    public Image abilityImage4;
    public TMP_Text abilityText4;
    public KeyCode ability4Key = KeyCode.R;
    public float ability4Cooldown;

    public Canvas ability4Canvas;
    public Image ability4Indicator;

    private bool isAbility1Cooldown = false;
    private bool isAbility3Cooldown = false;
    private bool isAbility4Cooldown = false;

    private float currentAbility1Cooldown;
    private float currentAbility3Cooldown;
    private float currentAbility4Cooldown;

    private Vector3 position;
    private RaycastHit hit;
    private Ray ray;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        stats = GetComponent<PlayerPrefab>();

        // Shows UI
        NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetChild(0).gameObject.SetActive(true);
        NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetChild(1).gameObject.SetActive(true);
        NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetChild(2).gameObject.SetActive(true);
        NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetChild(3).gameObject.SetActive(false);
        //NetworkManager.Singleton.LocalClient.PlayerObject.transform.GetChild(4).gameObject.SetActive(true);

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

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // TODO: Cast ability functionality
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
        while(Time.time < startTime + ABILITY1DASHTIME)
        {
            GetComponent<CharacterController>().Move(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position) * Vector3.forward * ABILITY1DASHSPEED * Time.deltaTime);
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

            SummonAbility2ParticlesServerRpc(ability2Active);
 
        }
    }
    private IEnumerator Ability2Interval()
    {
        nextTickTime = Time.time + ABILITY2TICKINTERVAL;

        // Trigger animation for auto attacking
        //anim.SetBool("isAttacking", true);

        // Wait based on atk speed / interval value
        // HANDLE DAMAGING
        GameManager.Instance.TakeDamage(gameObject, ABILITY2ACTIVATIONCOST * stats.MaxHealth);
        foreach (GameObject player in GameManager.Instance.playerPrefabs)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= ABILITY2RANGE)
            {
                GameManager.Instance.TakeDamage(player, stats.Damage);
            }
        }
        yield return new WaitForSeconds(ABILITY2TICKINTERVAL);
    }

    [ServerRpc]
    private void SummonAbility2ParticlesServerRpc(bool active)
    {
        SummonAbility2ParticlesClientRpc(active);
    }

    [ClientRpc]
    private void SummonAbility2ParticlesClientRpc(bool active)
    {
        transform.GetChild(3).gameObject.SetActive(active);
    }

    private void Ability3Input()
    {
        if (Input.GetKeyDown(ability3Key) && !isAbility3Cooldown)
        {
            playerMovement.StopMovement();

            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

            ability4Canvas.enabled = false;
            ability4Indicator.enabled = false;

            isAbility3Cooldown = true;
            currentAbility3Cooldown = ability3Cooldown;

            CastAbility3ServerRpc();
        }
    }

    // summon projectile here
    [ServerRpc]
    private void CastAbility3ServerRpc()
    {
        // ability cost: 30% of CURRENT HP
        GameManager.Instance.TakeDamage(gameObject, stats.Health * ABILITY3ACTIVATIONCOST);
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
        if (ability4Canvas.enabled && Input.GetMouseButtonDown(0))
        {
            isAbility4Cooldown = true;
            currentAbility4Cooldown = ability4Cooldown;

            ability4Canvas.enabled = false;
            ability4Indicator.enabled = false;
        }
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
