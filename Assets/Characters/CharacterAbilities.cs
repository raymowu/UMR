
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public abstract class CharacterAbilities : NetworkBehaviour
{
    [SerializeField] protected Canvas abilitiesCanvas;
    [SerializeField] protected Transform shootTransform;
    protected PlayerMovement playerMovement;
    protected Animator anim;
    protected PlayerPrefab stats;

    protected bool toggleActive = false;
    protected float nextTickTime = 0f;

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
        if (GetComponent<PlayerPrefab>().IsDead)
        {
            return;
        }
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

    protected void SummonThingCanvas(Canvas abilityCanvas, float range)
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

    protected void PointAndClickCanvas(Canvas abilityCanvas)
    {
        if (abilityCanvas.enabled)
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
            }
            Quaternion ab2Canvas = Quaternion.LookRotation(position - transform.position);
            ab2Canvas.eulerAngles = new Vector3(0, ab2Canvas.eulerAngles.y, ab2Canvas.eulerAngles.z);

            abilityCanvas.transform.rotation = Quaternion.Lerp(ab2Canvas, abilityCanvas.transform.rotation, 0);
        }
    }

    protected virtual void Ability1Canvas() { }
    protected virtual void Ability2Canvas() { }
    protected virtual void Ability3Canvas() { }
    protected virtual void Ability4Canvas() { }

    // using player as reference point
    public GameObject GetNearestEnemyInRange(float range)
    {
        GameObject tMin = null;
        float minDist = Mathf.Infinity;
        foreach (KeyValuePair<ulong, GameManager.Player> p in GameManager.Instance.playerPrefabs)
        {
            GameObject player = p.Value.playerObject;
            if (player == gameObject) { continue; }
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist <= range && dist < minDist)
            {
                tMin = player;
                minDist = dist;
            }
        }        
        foreach (KeyValuePair<ulong, GameManager.Mob> m in GameManager.Instance.mobPrefabs)
        {
            GameObject mob = m.Value.mobObject;
            if (mob == gameObject) { continue; }
            float dist = Vector3.Distance(mob.transform.position, transform.position);
            if (dist <= range && dist < minDist)
            {
                tMin = mob;
                minDist = dist;
            }
        }
        return tMin;
    }     
    public List<GameObject> GetAllEnemiesInRangeAndWithinAngle(float range, float ang)
    {
        List<GameObject> res = new List<GameObject> { };
        foreach (KeyValuePair<ulong, GameManager.Player> p in GameManager.Instance.playerPrefabs)
        {
            GameObject player = p.Value.playerObject;
            if (player == gameObject) { continue; }
            Vector3 directionToTarget = transform.position - player.transform.position;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            float distance = directionToTarget.magnitude;

            // Target is in front of me
            if (Mathf.Abs(angle) > ang && distance < range)
            {
                res.Add(player);
            }
        }        
        foreach (KeyValuePair<ulong, GameManager.Mob> m in GameManager.Instance.mobPrefabs)
        {
            GameObject mob = m.Value.mobObject;
            if (mob == gameObject) { continue; }
            Vector3 directionToTarget = transform.position - mob.transform.position;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            float distance = directionToTarget.magnitude;

            // Target is in front of me
            if (Mathf.Abs(angle) > ang && distance < range)
            {
                res.Add(mob);
            }
        }
        return res;
    }       
    
    // using specified parent object as reference point
    public GameObject GetNearestEnemyInRange(float range, GameObject parent)
    {
        GameObject tMin = null;
        float minDist = Mathf.Infinity;
        foreach (KeyValuePair<ulong, GameManager.Player> p in GameManager.Instance.playerPrefabs)
        {
            GameObject player = p.Value.playerObject;
            if (player == parent) { continue; }
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist <= range && dist < minDist)
            {
                tMin = player;
                minDist = dist;
            }
        } 
        foreach (KeyValuePair<ulong, GameManager.Player> m in GameManager.Instance.playerPrefabs)
        {
            GameObject mob = m.Value.playerObject;
            if (mob == parent) { continue; }
            float dist = Vector3.Distance(mob.transform.position, transform.position);
            if (dist <= range && dist < minDist)
            {
                tMin = mob;
                minDist = dist;
            }
        }
        return tMin;
    }    
    
    // using player as reference point
    public List<GameObject> GetAllEnemiesInRange(float range)
    {
        List<GameObject> res = new List<GameObject> { };
        foreach (KeyValuePair<ulong, GameManager.Player> p in GameManager.Instance.playerPrefabs)
        {
            GameObject player = p.Value.playerObject;
            if (player == gameObject) { continue; }
            if (Vector3.Distance(transform.position, player.transform.position) <= range)
            {
                res.Add(player);
            }
        }
        foreach (KeyValuePair<ulong, GameManager.Mob> m in GameManager.Instance.mobPrefabs)
        {
            GameObject mob = m.Value.mobObject;
            if (mob == gameObject) { continue; }
            if (Vector3.Distance(transform.position, mob.transform.position) <= range)
            {
                res.Add(mob);
            }
        }
        return res;
    }       

    // using specified parent object as reference point
    public List<GameObject> GetAllPlayersInRange(float range, GameObject parent)
    {
        List<GameObject> res = new List<GameObject> { };
        foreach (KeyValuePair<ulong, GameManager.Player> p in GameManager.Instance.playerPrefabs)
        {
            GameObject player = p.Value.playerObject;
            if (Vector3.Distance(parent.transform.position, player.transform.position) <= range)
            {
                res.Add(player);
            }
        }
        return res;
    }    
    
    public List<GameObject> GetAllPlayers()
    {
        List<GameObject> res = new List<GameObject> { };
        foreach (KeyValuePair<ulong, GameManager.Player> p in GameManager.Instance.playerPrefabs)
        {
            GameObject player = p.Value.playerObject;
            res.Add(player);
        }
        return res;
    }
    // QUESTION: do i need abilitycooldown to even be passed
    // ANSWER: yes bc that determines which ability's abilitycooldown to use
    // QUESTION: does abilitycooldown need to be ref bc its just in the parent class
    // ANSWER: yes bc of abilityCooldown setting cooldown timers

    // Hold to aim then lift button abilities
    protected void InputHelper(KeyCode abilityKey, ref bool isAbilityCooldown, Canvas abilityCanvas, 
        float abilityCooldown, ref float currentAbilityCooldown, string animTrigger, Action callback)
    {

        if (Input.GetKeyDown(abilityKey) && !isAbilityCooldown)
        {
            // QUESTION: do i even need to disable everything cant i just disable go in unity and enable only the one i need
            // QUESTION: will .enabled work if canvas is not set/ doesnt exist
            // ANSWER: no
            ability1IndicatorCanvas.enabled = false;
            ability2IndicatorCanvas.enabled = false;
            ability3IndicatorCanvas.enabled = false;
            ability4IndicatorCanvas.enabled = false;

            abilityCanvas.enabled = true;
        }

        if (abilityCanvas.enabled && Input.GetKeyUp(abilityKey))
        {

            isAbilityCooldown = true;
            currentAbilityCooldown = abilityCooldown;
            callback?.Invoke();
            anim.SetTrigger(animTrigger);

            abilityCanvas.enabled = false;
        }
    }

    // Just press button no aim abilities
    protected void InputHelper(KeyCode abilityKey, ref bool isAbilityCooldown, float abilityCooldown, 
        ref float currentAbilityCooldown, string animTrigger, Action callback)
    {
        if (Input.GetKeyDown(abilityKey) && !isAbilityCooldown)
        {
            ability1IndicatorCanvas.enabled = false;
            ability2IndicatorCanvas.enabled = false;
            ability3IndicatorCanvas.enabled = false;
            ability4IndicatorCanvas.enabled = false;

            isAbilityCooldown = true;
            currentAbilityCooldown = abilityCooldown;
            callback?.Invoke();
            anim.SetTrigger(animTrigger);
        }
    }

    // point and click
    protected void InputHelper(KeyCode abilityKey, ref bool isAbilityCooldown, Canvas abilityCanvas, 
        float abilityCooldown, ref float currentAbilityCooldown, string animTrigger, float range, 
        Action callback)
    {
        if (Input.GetKeyDown(abilityKey) && !isAbility2Cooldown)
        {
            ability1IndicatorCanvas.enabled = false;
            ability2IndicatorCanvas.enabled = false;
            ability3IndicatorCanvas.enabled = false;
            ability4IndicatorCanvas.enabled = false;
            abilityCanvas.enabled = true;
        }
        GameObject targetEnemy = playerMovement.targetEnemy;
        if (!isAbilityCooldown && abilityCanvas.enabled && targetEnemy != null && 
            Vector3.Distance(transform.position, targetEnemy.transform.position) <= range)
        {
            isAbilityCooldown = true;
            currentAbilityCooldown = abilityCooldown;

            abilityCanvas.enabled = false;

            callback?.Invoke();
        }
        if (Input.GetKeyUp(abilityKey))
        {
            abilityCanvas.enabled = false;
        }
    }

    private IEnumerator ToggleInterval(float tickInterval, Action callback)
    {
        nextTickTime = Time.time + tickInterval;
        callback?.Invoke();
        
        yield return new WaitForSeconds(tickInterval);
    }

    // callback is code to execute while toggle is on
    protected void ToggleInputHelper(KeyCode abilityKey, float tickInterval, Action intervalCallback, Action callback)
    {
        if (toggleActive && Time.time > nextTickTime)
        {
            StartCoroutine(ToggleInterval(tickInterval, intervalCallback));
        }

        if (Input.GetKeyDown(abilityKey))
        {
            abilityImage2.fillAmount = toggleActive ? 0 : 1;
            toggleActive = !toggleActive;
            callback?.Invoke();
        }
    }    

    protected virtual void Ability1Input() {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1Cooldown, ref currentAbility1Cooldown, "", () => { });
    }

    protected virtual void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2Cooldown, ref currentAbility2Cooldown, "", () => { });
    }


    protected virtual void Ability3Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3Cooldown, ref currentAbility3Cooldown, "", () => { });
    }

    protected virtual void Ability4Input()
    {
        InputHelper(ability4Key, ref isAbility4Cooldown, ability4Cooldown, ref currentAbility4Cooldown, "", () => { });
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
