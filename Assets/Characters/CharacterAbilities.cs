
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public abstract class CharacterAbilities : NetworkBehaviour
{
    [SerializeField] protected Canvas abilitiesCanvas;
    [SerializeField] protected Transform shootTransform;
    protected PlayerMovement playerMovement;
    protected Animator anim;
    protected PlayerPrefab stats;

    [Header("Ability 1")]
    public Image abilityImage1;
    public TMP_Text abilityText1;
    public KeyCode ability1Key = KeyCode.Q;
    public float ability1Cooldown;
    public Canvas ability1IndicatorCanvas;
    public GameObject ability1DisableOverlay;

    [Header("Ability 2")]
    public Image abilityImage2;
    public TMP_Text abilityText2;
    public KeyCode ability2Key = KeyCode.W;
    public float ability2Cooldown;
    public Canvas ability2IndicatorCanvas;
    public GameObject ability2DisableOverlay;

    [Header("Ability 3")]
    public Image abilityImage3;
    public TMP_Text abilityText3;
    public KeyCode ability3Key = KeyCode.E;
    public float ability3Cooldown;
    public Canvas ability3IndicatorCanvas;
    public GameObject ability3DisableOverlay;

    [Header("Ability 4")]
    public Image abilityImage4;
    public TMP_Text abilityText4;
    public KeyCode ability4Key = KeyCode.R;
    public float ability4Cooldown;
    public Canvas ability4IndicatorCanvas;
    public GameObject ability4DisableOverlay;

    protected bool isAbility1Cooldown = false;
    protected bool isAbility2Cooldown = false;
    protected bool isAbility3Cooldown = false;
    protected bool isAbility4Cooldown = false;

    protected float currentAbility1Cooldown;
    protected float currentAbility2Cooldown;
    protected float currentAbility3Cooldown;
    protected float currentAbility4Cooldown;

    protected Vector3 position;
    protected RaycastHit hit;
    protected Ray ray;

    // Start is called before the first frame update
    protected void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        stats = GetComponent<PlayerPrefab>();
        // Shows UI
        if (IsOwner)
        {
            abilitiesCanvas.gameObject.SetActive(true);
            ability1IndicatorCanvas.gameObject.SetActive(true);
            ability2IndicatorCanvas.gameObject.SetActive(true);
            ability3IndicatorCanvas.gameObject.SetActive(true);
            ability4IndicatorCanvas.gameObject.SetActive(true);
        }

        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;
        abilityImage3.fillAmount = 0;
        abilityImage4.fillAmount = 0;

        abilityText1.text = "";
        abilityText2.text = "";
        abilityText3.text = "";
        abilityText4.text = "";

        ability1IndicatorCanvas.enabled = false;
        ability2IndicatorCanvas.enabled = false;
        ability3IndicatorCanvas.enabled = false;
        ability4IndicatorCanvas.enabled = false;
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

        Ability1Canvas();
        Ability2Canvas();
        Ability3Canvas();
        Ability4Canvas();
    }

    /* INDICATORS */
    protected void LinearProjectileCanvas(Canvas abilityCanvas)
    {
        if (abilityCanvas.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }
            Quaternion ab1Canvas = Quaternion.LookRotation(position - transform.position);
            ab1Canvas.eulerAngles = new Vector3(0, ab1Canvas.eulerAngles.y, ab1Canvas.eulerAngles.z);

            abilityCanvas.transform.rotation = Quaternion.Lerp(ab1Canvas, abilityCanvas.transform.rotation, 0);
        }
    }

    protected void SummonProjectileCanvas(Canvas abilityCanvas, float range)
    {
        int layerMask = ~LayerMask.GetMask("Player");
        if (abilityCanvas.enabled)
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
            distance = Mathf.Min(distance, range);

            var newHitPos = new Vector3(transform.position.x, 0.2f, transform.position.z) + hitPosDir * distance;
            abilityCanvas.transform.position = (newHitPos);
        }
    }

    protected abstract void Ability1Canvas();
    protected abstract void Ability2Canvas();
    protected abstract void Ability3Canvas();
    protected abstract void Ability4Canvas();

    protected void InputHelper(KeyCode abilityKey, ref bool isAbilityCooldown, Canvas abilityCanvas, 
        ref float abilityCooldown, ref float currentAbilityCooldown, Action callback)
    {

        if (Input.GetKeyDown(abilityKey) && !isAbilityCooldown)
        {
            ability1IndicatorCanvas.enabled = false;
            ability2IndicatorCanvas.enabled = false;
            ability3IndicatorCanvas.enabled = false;
            ability4IndicatorCanvas.enabled = false;

            Cursor.visible = false;

            abilityCanvas.enabled = true;
        }

        if (abilityCanvas.enabled && Input.GetKeyUp(abilityKey))
        {
            callback?.Invoke();

            isAbilityCooldown = true;
            currentAbilityCooldown = abilityCooldown;

            abilityCanvas.enabled = false;

            Cursor.visible = true;
        }
    }

    protected virtual void Ability1Input() {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ref ability1Cooldown,
                   ref currentAbility1Cooldown, () => {});
    }

    protected virtual void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ref ability2Cooldown,
                   ref currentAbility2Cooldown, () => { });
    }


    protected virtual void Ability3Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3IndicatorCanvas, ref ability3Cooldown,
                   ref currentAbility3Cooldown, () => { });
    }

    protected virtual void Ability4Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3IndicatorCanvas, ref ability3Cooldown,
                  ref currentAbility3Cooldown, () => { });
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
