using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PlayerMovement)), RequireComponent(typeof(Stats))]
public class MeleeCombat : NetworkBehaviour
{
    private PlayerMovement moveScript;
    private Stats stats;
    private Animator anim;

    [Header("Target")]
    public GameObject targetEnemy;

    [Header("Melee Attack Variables")]
    public bool performMeleeAttack = true;
    private float attackInterval;
    private float nextAttackTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        moveScript = GetComponent<PlayerMovement>();
        stats = GetComponent<Stats>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return;  }
        // Calculates atk speed and interval between auto attacks
        attackInterval = stats.attackSpeed / ((500 + stats.attackSpeed) * 0.01f);

        targetEnemy = moveScript.targetEnemy;

        // Perform the Melee auto attack if in range
        if (targetEnemy != null && targetEnemy != NetworkManager.Singleton.LocalClient.PlayerObject.gameObject && 
            performMeleeAttack && Time.time > nextAttackTime)
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

        // Checking if the enemy is still alive
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
        if (targetEnemy != null)
        {
            stats.TakeDamage(targetEnemy, stats.damage);
        }

        // Set the next attack time
        nextAttackTime = Time.time + attackInterval;
        performMeleeAttack = true;

        // Stop calling the auto atk animation
        anim.SetBool("isAttacking", false);
    }
}
