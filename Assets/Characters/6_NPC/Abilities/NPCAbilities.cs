using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NPCAbilities : NetworkBehaviour
{
    [SerializeField] private Transform shootTransform;
    [SerializeField] private Transform multiplyShootTransform;
    [SerializeField] private Transform wallShootTransform;
    [SerializeField] private Transform mentalDrainShootTransform;
    [SerializeField] private Canvas abilitiesCanvas;
    private PlayerMovement playerMovement;
    private Animator anim;
    private PlayerPrefab stats;

    [Header("Ability 1")]
    [SerializeField] private GameObject NPCClone;
    public float MULTIPLY_RANGE = 5f;
    public Image abilityImage1;
    public TMP_Text abilityText1;
    public KeyCode ability1Key = KeyCode.Q;
    public float ability1Cooldown;
    public Canvas ability1Canvas;
    public Image ability1Indicator;
    public Canvas ability1RangeIndicatorCanvas;
    public Image ability1RangeIndicator;
    public GameObject ability1DisableOverlay;

    [Header("Ability 2")]
    [SerializeField] private GameObject NPCWall;
    public Image abilityImage2;
    public TMP_Text abilityText2;
    public KeyCode ability2Key = KeyCode.W;
    public float ability2Cooldown;
    public Canvas ability2Canvas;
    public Image ability2Indicator;
    public GameObject ability2DisableOverlay;

    [Header("Ability 3")]
    [SerializeField] private GameObject MentalDrainParticles;
    public float MENTAL_DRAIN_RANGE = 5f;
    public float MENTAL_DRAIN_DURATION = 5f;
    public float MENTAL_DRAIN_RADIUS = 2.5f;
    public float MENTAL_DRAIN_SLOW_AMOUNT = 0.5f;
    public float MENTAL_DRAIN_SLOW_DURATION = 0.5f;
    public float MENTAL_DRAIN_DAMAGE = 10f;
    public float MENTAL_DRAIN_TICK_INTERVAL = 0.5f;
    public Image abilityImage3;
    public TMP_Text abilityText3;
    public KeyCode ability3Key = KeyCode.E;
    public float ability3Cooldown;
    public Canvas ability3Canvas;
    public Image ability3Indicator;
    public Canvas ability3RangeIndicatorCanvas;
    public Image ability3RangeIndicator;
    public GameObject ability3DisableOverlay;

    [Header("Ability 4")]
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

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        stats = GetComponent<PlayerPrefab>();

        if (IsOwner)
        {
            abilitiesCanvas.gameObject.SetActive(true);
            ability1Canvas.gameObject.SetActive(true);
            ability1RangeIndicatorCanvas.gameObject.SetActive(true);    
            ability2Canvas.gameObject.SetActive(true);
            ability3Canvas.gameObject.SetActive(true);
            ability3RangeIndicatorCanvas.gameObject.SetActive(true);
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
        ability1RangeIndicator.enabled = false;
        ability2Indicator.enabled = false;
        ability3Indicator.enabled = false;
        ability3RangeIndicator.enabled = false;

        ability1Canvas.enabled = false;
        ability1RangeIndicatorCanvas.enabled = false;
        ability2Canvas.enabled = false;
        ability3Canvas.enabled = false;
        ability3RangeIndicatorCanvas.enabled = false;
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
        int layerMask = ~LayerMask.GetMask("Player");
        if (ability1Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    position = hit.point;
                }
            }

            var hitPosDir = (hit.point - transform.position).normalized;
            float distance = Vector3.Distance(hit.point, transform.position);
            distance = Mathf.Min(distance, MULTIPLY_RANGE);

            var newHitPos = new Vector3(transform.position.x, 0.2f, transform.position.z) + hitPosDir * distance;
            ability1Canvas.transform.position = (newHitPos);
        }
    }

    private void Ability2Canvas()
    {
        if (ability2Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, 0f, hit.point.z);
            }
            Quaternion ab2Canvas = Quaternion.LookRotation(position - transform.position);
            ab2Canvas.eulerAngles = new Vector3(0, ab2Canvas.eulerAngles.y, ab2Canvas.eulerAngles.z);

            ability2Canvas.transform.rotation = Quaternion.Lerp(ab2Canvas, ability2Canvas.transform.rotation, 0);
        }
    }

    private void Ability3Canvas()
    {
        int layerMask = ~LayerMask.GetMask("Player");
        if (ability3Indicator.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    position = hit.point;
                }
            }

            var hitPosDir = (hit.point - transform.position).normalized;
            float distance = Vector3.Distance(hit.point, transform.position);
            distance = Mathf.Min(distance, MENTAL_DRAIN_RANGE);

            var newHitPos = new Vector3(transform.position.x, 0.2f, transform.position.z) + hitPosDir * distance;
            ability3Canvas.transform.position = (newHitPos);
        }
    }

    private void Ability1Input()
    {
        if (Input.GetKeyDown(ability1Key) && !isAbility1Cooldown)
        {
            ability1Canvas.enabled = true;
            ability1RangeIndicatorCanvas.enabled = true;
            ability1Indicator.enabled = true;
            ability1RangeIndicator.enabled = true;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
            ability3RangeIndicator.enabled = false;
            ability3RangeIndicatorCanvas.enabled = false;

            Cursor.visible = false;
        }

        if (ability1Canvas.enabled && Input.GetKeyUp(ability1Key))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                playerMovement.StopMovement();
                playerMovement.Rotate(hit.point);
                float distance = Vector3.Distance(hit.point, transform.position);
                Vector3 spawnClonePosition = distance <= MULTIPLY_RANGE ? hit.point : multiplyShootTransform.position;
                    SummonNPCCloneServerRpc(new Vector3(spawnClonePosition.x, 0f, spawnClonePosition.z), 
                    Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) - transform.position));
            }

            isAbility1Cooldown = true;
            currentAbility1Cooldown = ability1Cooldown;

            ability1Canvas.enabled = false;
            ability1RangeIndicatorCanvas.enabled = false;
            ability1Indicator.enabled = false;
            ability1RangeIndicator.enabled = false;

            Cursor.visible = true;
        }
    }

    [ServerRpc]
    private void SummonNPCCloneServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(NPCClone, new Vector3(pos.x, pos.y, pos.z), rot);
        go.GetComponent<MeleeMobAI>().parent = gameObject;
        go.GetComponent<AIMeleeCombat>().parent = gameObject;
        go.GetComponent<NetworkObject>().Spawn();
    }

    private void Ability2Input()
    {
        if (Input.GetKeyDown(ability2Key) && !isAbility2Cooldown)
        {
            ability2Canvas.enabled = true;
            ability2Indicator.enabled = true;

            ability1Canvas.enabled = false;
            ability1RangeIndicatorCanvas.enabled = false;
            ability1Indicator.enabled = false;
            ability1RangeIndicator.enabled = false;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
            ability3RangeIndicator.enabled = false;
            ability3RangeIndicatorCanvas.enabled = false;
        }

        if (ability2Canvas.enabled && Input.GetKeyUp(ability2Key))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                playerMovement.StopMovement();
                playerMovement.Rotate(hit.point);
                SummonNPCWallServerRpc(hit.point, Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) -
                    new Vector3(transform.position.x, 0f, transform.position.z)));
            }

            isAbility2Cooldown = true;
            currentAbility2Cooldown = ability2Cooldown;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;
        }
    }

    [ServerRpc]
    private void SummonNPCWallServerRpc(Vector3 pos, Quaternion rot)
    {
        // Need to rotate player in server RPC so the position is correct realtime value for server
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(NPCWall, new Vector3(wallShootTransform.position.x, 0f, wallShootTransform.position.z), rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<NetworkObject>().Spawn();
    }

    private void Ability3Input()
    {
        if (Input.GetKeyDown(ability3Key) && !isAbility3Cooldown)
        {
            ability3Canvas.enabled = true;
            ability3Indicator.enabled = true;
            ability3RangeIndicator.enabled = true;
            ability3RangeIndicatorCanvas.enabled = true;

            ability1Canvas.enabled = false;
            ability1RangeIndicatorCanvas.enabled = false;
            ability1Indicator.enabled = false;
            ability1RangeIndicator.enabled = false;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;

            Cursor.visible = false;
        }
        if (ability3Canvas.enabled && Input.GetKeyUp(ability3Key))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                playerMovement.StopMovement();
                playerMovement.Rotate(hit.point);
                float distance = Vector3.Distance(hit.point, transform.position);
                Vector3 spawnMentalDrainPosition = distance <= MULTIPLY_RANGE ? hit.point : mentalDrainShootTransform.position;
                SummonMentalDrainParticlesServerRpc(new Vector3(spawnMentalDrainPosition.x, .1f, spawnMentalDrainPosition.z),
                    Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) - transform.position));
            }

            isAbility3Cooldown = true;
            currentAbility3Cooldown = ability3Cooldown;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
            ability3RangeIndicator.enabled = false;
            ability3RangeIndicatorCanvas.enabled = false;

            Cursor.visible = true;
        }
    }

    [ServerRpc]
    private void SummonMentalDrainParticlesServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(hit.point);
        GameObject go = Instantiate(MentalDrainParticles, pos, rot);
        go.GetComponent<HandleMentalDrainCollision>().parent = this;
        go.GetComponent<AutoDestroyGameObject>().delayBeforeDestroy = MENTAL_DRAIN_DURATION;
        go.GetComponent<NetworkObject>().Spawn();
    }

    private void Ability4Input()
    {
        if (Input.GetKeyDown(ability4Key) && !isAbility4Cooldown)
        {
            ability1Canvas.enabled = false;
            ability1RangeIndicatorCanvas.enabled = false;
            ability1Indicator.enabled = false;
            ability1RangeIndicator.enabled = false;

            ability2Canvas.enabled = false;
            ability2Indicator.enabled = false;

            ability3Canvas.enabled = false;
            ability3Indicator.enabled = false;
            ability3RangeIndicator.enabled = false;
            ability3RangeIndicatorCanvas.enabled = false;

            isAbility4Cooldown = true;
            currentAbility4Cooldown = ability4Cooldown;

            playerMovement.StopMovement();
            playerMovement.Rotate(hit.point);
            SummonNPCCloneServerRpc(new Vector3(transform.position.x + 2f, 0f, transform.position.z),
                transform.rotation);
            SummonNPCCloneServerRpc(new Vector3(transform.position.x - 2f, 0f, transform.position.z),
                transform.rotation);
            SummonNPCCloneServerRpc(new Vector3(transform.position.x, 0f, transform.position.z + 2f),
                transform.rotation);
            SummonNPCCloneServerRpc(new Vector3(transform.position.x, 0f, transform.position.z - 2f),
                transform.rotation);
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
