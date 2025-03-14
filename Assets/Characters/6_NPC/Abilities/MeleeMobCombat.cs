using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(MeleeMobAI)), RequireComponent(typeof(PlayerPrefab))]
public class MeleeMobCombat : NetworkBehaviour
{
    private MeleeMobAI moveScript;
    private PlayerPrefab stats;
    private Animator anim;

    [Header("Target")]
    public GameObject targetEnemy;

    [Header("Melee Attack Variables")]
    public GameObject parent;
    public bool performMeleeAttack = true;
    private float attackInterval;
    private float nextAttackTime = 0;

    void Start()
    {
        moveScript = GetComponent<MeleeMobAI>();
        stats = GetComponent<PlayerPrefab>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) { return; }
        if (parent && targetEnemy == parent) { return; }
        if (stats.IsDisarmed) { return; }

        // Calculates atk speed and interval between auto attacks
        attackInterval = stats.AttackSpeed / ((500 + stats.AttackSpeed) * 0.01f);

        targetEnemy = moveScript.targetEnemy;

        // Perform the Melee auto attack if in range
        if (targetEnemy != null && performMeleeAttack && Time.time > nextAttackTime && targetEnemy.layer != LayerMask.NameToLayer("Ignore Raycast"))
        {
            if (Vector3.Distance(transform.position, targetEnemy.transform.position) <= moveScript.stoppingDistance)
            {
                StartCoroutine(MeleeAttackInterval());
            }
        }
    }

    private IEnumerator MeleeAttackInterval()
    {
        performMeleeAttack = false;

        // Trigger animation for auto attacking
        anim.SetBool("isAttacking", true);

        // Wait based on atk speed / interval value
        yield return new WaitForSeconds(attackInterval);

        if (targetEnemy == null)
        {
            // Stop animation bool and let it go back to being able to atk
            anim.SetBool("isAttacking", false);
            performMeleeAttack = true;
        }
    }

    // CALL IN THE ANIMATION EVENT
    private void MeleeAttack()
    {
        if (!IsOwner) { return; }
        //if (parent && targetEnemy == parent) { return; }
        if (stats.IsDisarmed) { return; }

        if (targetEnemy != null)
        {
            GameManager.Instance.DealDamage(parent.gameObject, targetEnemy, stats.Damage); // TODO: see if it links to owner
        }

        // Set the next attack time
        nextAttackTime = Time.time + attackInterval;
        performMeleeAttack = true;

        // Stop calling the auto atk animation
        anim.SetBool("isAttacking", false);
    }
}
