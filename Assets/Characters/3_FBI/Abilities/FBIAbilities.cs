
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class FBIAbilities : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    [SerializeField] private Canvas abilitiesCanvas;
    private PlayerMovement playerMovement;
    private PlayerPrefab stats;
    private PlayerMovement moveScript;

    [Header("Ability 1")]
    [SerializeField] private GameObject ability1Projectile;
    public float MAGNUM_SHOT_DAMAGE = 30f;
    public Image abilityImage1;
    public TMP_Text abilityText1;
    public KeyCode ability1Key = KeyCode.Q;
    public float ability1Cooldown;
    public Canvas ability1Canvas;
    public Image ability1Indicator;
    public GameObject ability1DisableOverlay;

    [Header("Ability 2")]
    [SerializeField] private GameObject ability2Projectile;
    public float FBI_OPEN_UP_DAMAGE = 10f;
    public float FBI_OPEN_UP_SLOW_AMOUNT = .6f;
    public float FBI_OPEN_UP_SLOW_DURATION = 2f;
    public Image abilityImage2;
    public TMP_Text abilityText2;
    public KeyCode ability2Key = KeyCode.W;
    public float ability2Cooldown;
    public Canvas ability2Canvas;
    public Image ability2Indicator;
    public GameObject ability2DisableOverlay;

    [Header("Ability 3")]
    public Image abilityImage3;
    public TMP_Text abilityText3;
    public KeyCode ability3Key = KeyCode.E;
    public float ability3Cooldown;
    public Canvas ability3Canvas;
    public Image ability3Indicator;
    public GameObject ability3DisableOverlay;
    public float TAZE_RANGE = 5f;
    public float TAZE_DURATION = 1.5f;

    [Header("Ability 4")]
    [SerializeField] private GameObject policeCar;
    public Image abilityImage4;
    public TMP_Text abilityText4;
    public KeyCode ability4Key = KeyCode.R;
    public float ability4Cooldown;
    public GameObject ability4DisableOverlay;
    public float HIGH_SPEED_CHASE_DURATION = 10f;
    public float HIGH_SPEED_CHASE_SPEED = 3f;
    public float HIGH_SPEED_CHASE_COLLISION_DAMAGE = 60f;
    private bool highSpeedChaseActive = false;

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
        moveScript = GetComponent<PlayerMovement>();

        if (IsOwner)
        {
            abilitiesCanvas.gameObject.SetActive(true);
            ability1Canvas.gameObject.SetActive(true);
            ability2Canvas.gameObject.SetActive(true);
            ability3Canvas.gameObject.SetActive(true);
        }

        policeCar.gameObject.SetActive(false);

        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;
        abilityImage3.fillAmount = 0;
        abilityImage4.fillAmount = 0;

        abilityText1.text = "";
        abilityText2.text = "";
        abilityText3.text = "";
        abilityText4.text = "";

        ability1Indicator.enabled = false;
        ability2Indicator.enabled = false;
        ability3Indicator.enabled = false;

        ability1Canvas.enabled = false;
        ability2Canvas.enabled = false;
        ability3Canvas.enabled = false;
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
        AbilityCooldown(ref currentAbility2Cooldown, ability2Cooldown, ref isAbility2Cooldown, abilityImage2, abilityText2);
        AbilityCooldown(ref currentAbility3Cooldown, ability3Cooldown, ref isAbility3Cooldown, abilityImage3, abilityText3);
        AbilityCooldown(ref currentAbility4Cooldown, ability4Cooldown, ref isAbility4Cooldown, abilityImage4, abilityText4);

        Ability1Canvas();
        Ability2Canvas();
        Ability3Canvas();
    }

    private void Ability1Canvas()
    {
        if (ability1Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }
            Quaternion ab1Canvas = Quaternion.LookRotation(position - transform.position);
            ab1Canvas.eulerAngles = new Vector3(0, ab1Canvas.eulerAngles.y, ab1Canvas.eulerAngles.z);

            ability1Canvas.transform.rotation = Quaternion.Lerp(ab1Canvas, ability1Canvas.transform.rotation, 0);
        }
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
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
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
            ability1Canvas.enabled = true;
            ability1Indicator.enabled = true;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
        }

        if (ability1Indicator.enabled && Input.GetKeyUp(ability1Key))
        {
            // Raymond note: There was a bug with the raycast hit hitting the player prefab and using that y value, 
            // which sends the projectile into the air cuz the click was on top of a guy. There r prob 2 ways to solve this,
            // either set the layer mask to only get the ground layer OR dont use the hit's y value
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                playerMovement.StopMovement();
                playerMovement.Rotate(hit.point);
                CastAbility1ServerRpc(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
            }

            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;

            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

        }
    }

    // summon projectile here
    [ServerRpc]
    private void CastAbility1ServerRpc(Quaternion rot)
    {
        GameObject go = Instantiate(ability1Projectile, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveBullet>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    private void Ability2Input()
    {
        if (Input.GetKeyDown(ability2Key) && !isAbility2Cooldown)
        {
            ability2Canvas.enabled = true;
            ability2Indicator.enabled = true;

            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
        }
        if (ability2Canvas.enabled && Input.GetKeyUp(ability2Key))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                playerMovement.StopMovement();
                playerMovement.Rotate(hit.point);
                CastAbility2ServerRpc(hit.point, Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
            }

            isAbility2Cooldown = true;
            currentAbility2Cooldown = ability2Cooldown;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;

            GetComponent<OwnerNetworkAnimator>().SetTrigger("CastFBIOpenUp");
        }
    }

    [ServerRpc]
    private void CastAbility2ServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(ability2Projectile, new Vector3(shootTransform.position.x, 0.8f, shootTransform.position.z), rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveDoor>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    private void Ability3Input()
    {
        if (Input.GetKeyDown(ability3Key) && !isAbility3Cooldown)
        {
            ability3Canvas.enabled = true;
            ability3Indicator.enabled = true;

            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;
        }
        GameObject targetEnemy = moveScript.targetEnemy;
        if (!isAbility2Cooldown && ability3Canvas.enabled && targetEnemy != null && Vector3.Distance(transform.position, targetEnemy.transform.position) <= TAZE_RANGE)
        {
            isAbility3Cooldown = true;
            currentAbility3Cooldown = ability3Cooldown;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;

            playerMovement.Rotate(hit.point);
            GameManager.Instance.Stun(targetEnemy, TAZE_DURATION);
            //TODO add taze particles
        }
        if (Input.GetKeyUp(ability3Key))
        {
            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
        }
    }

    private void Ability4Input()
    {
        if (highSpeedChaseActive)
        {
            foreach (GameObject player in GameManager.Instance.playerPrefabs)
            {
                if (player == gameObject) { return; }
                if (Vector3.Distance(transform.position, player.transform.position) <= 2f)
                {
                    highSpeedChaseActive = false;
                    GameManager.Instance.DealDamage(gameObject, player.gameObject, GetComponent<PlayerPrefab>().Damage + HIGH_SPEED_CHASE_COLLISION_DAMAGE);
                    GameManager.Instance.RemoveSlowsAndSpeeds(gameObject);
                    TogglePoliceCarServerRpc(false);
                }
            }
        }

        if (Input.GetKeyDown(ability4Key) && !isAbility4Cooldown)
        {
            ability1Canvas.enabled = false;
            ability1Indicator.enabled = false;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;

            isAbility4Cooldown = true;
            currentAbility4Cooldown = ability4Cooldown;

            highSpeedChaseActive = true;
            TogglePoliceCarServerRpc(true);
            GameManager.Instance.Speed(gameObject, HIGH_SPEED_CHASE_SPEED, HIGH_SPEED_CHASE_DURATION);
            StartCoroutine(DestroyPoliceCar());
        }
    }

    [ServerRpc]
    public void TogglePoliceCarServerRpc(bool active)
    {
        TogglePoliceCarClientRpc(active);
    }

    [ClientRpc]
    private void TogglePoliceCarClientRpc(bool active)
    {
        policeCar.gameObject.SetActive(active);
    }

    IEnumerator DestroyPoliceCar()
    {
        yield return new WaitForSeconds(HIGH_SPEED_CHASE_DURATION);
        GameManager.Instance.RemoveSlowsAndSpeeds(gameObject);
        TogglePoliceCarServerRpc(false);
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
