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
        if (targetEnemy != null)
        {
            SummonAutoProjectile(gameObject, targetEnemy);
        }
        // Set the next attack time
        nextAttackTime = Time.time + attackInterval;
        performRangedAttack = true;

        // Stop calling the auto atk animation
        anim.SetBool("isAttacking", false);
    }

    public void SummonAutoProjectile(GameObject parent, GameObject target)
    {
        SummonAutoProjectileServerRpc(parent.GetComponent<NetworkObject>().OwnerClientId, target.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc]
    private void SummonAutoProjectileServerRpc(ulong parentId, ulong targetId)
    {
        NetworkList<PlayerStats> players = GameManager.Instance.players;
        GameObject[] playerPrefabs = GameManager.Instance.playerPrefabs;
        GameObject parent = playerPrefabs[0];
        GameObject target = playerPrefabs[0];

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != parentId) { continue; }
            parent = playerPrefabs[i];
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != targetId) { continue; }
            target = playerPrefabs[i];
        }

        GameObject go = Instantiate(projectilePrefab, shootTransform.position, Quaternion.identity);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), parent.GetComponent<Collider>());
        go.GetComponent<MoveRangedAuto>().parent = parent;
        go.GetComponent<MoveRangedAuto>().target = target;
        go.GetComponent<NetworkObject>().Spawn();
    }
}
