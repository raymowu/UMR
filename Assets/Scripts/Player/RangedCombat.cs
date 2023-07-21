using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(PlayerMovement)), RequireComponent(typeof(PlayerPrefab))]
public class RangedCombat : NetworkBehaviour
{
    private PlayerMovement moveScript;
    private PlayerPrefab stats;
    private Animator anim;

    [Header("Target")]
    public GameObject targetEnemy;

    [Header("Ranged Attack Variables")]
    public bool performRangedAttack = true;
    public GameObject projectilePrefab;
    public Transform shootTransform;
    private float attackInterval;
    private float nextAttackTime = 0;

    void Start()
    {
        moveScript = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerPrefab>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) { return; }
        if (stats.IsDisarmed) { return;  }
        // Calculates atk speed and interval between auto attacks
        attackInterval = stats.AttackSpeed / ((500 + stats.AttackSpeed) * 0.01f);

        targetEnemy = moveScript.targetEnemy;

        // Perform the ranged auto attack if in range
        if (targetEnemy != null && targetEnemy != NetworkManager.Singleton.LocalClient.PlayerObject.gameObject &&
            performRangedAttack && Time.time > nextAttackTime)
        {
            if (Vector3.Distance(transform.position, targetEnemy.transform.position) <= moveScript.stoppingDistance)
            {
                StartCoroutine(RangedAttackInterval());
            }
        }
    }

    private IEnumerator RangedAttackInterval()
    {
        performRangedAttack = false;

        // Trigger animation for auto attacking
        anim.SetBool("isAttacking", true);

        // Wait based on atk speed / interval value
        yield return new WaitForSeconds(attackInterval);

        if (targetEnemy == null)
        {
            // Stop animation bool and let it go back to being able to atk
            anim.SetBool("isAttacking", false);
            performRangedAttack = true;
        }
    }

    // CALL IN THE ANIMATION EVENT
    private void RangedAttack()
    {
        if (!IsOwner) { return; }
        if (stats.IsDisarmed) { return; }

        if (targetEnemy != null)
        {
            SummonAutoProjectileServerRpc();
        }
        // Set the next attack time
        nextAttackTime = Time.time + attackInterval;
        performRangedAttack = true;

        // Stop calling the auto atk animation
        anim.SetBool("isAttacking", false);
    }

    [ServerRpc]
    private void SummonAutoProjectileServerRpc()
    {
        GameObject go = Instantiate(projectilePrefab, shootTransform.position, Quaternion.identity);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveRangedAuto>().parent = gameObject;
        go.GetComponent<MoveRangedAuto>().target = targetEnemy;
        go.GetComponent<NetworkObject>().Spawn();
    }
}
