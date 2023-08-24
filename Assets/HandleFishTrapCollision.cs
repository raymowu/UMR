using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandleFishTrapCollision : NetworkBehaviour
{
    public KleeAbilities parent;
    private float nextTickTime = 0f;
    private float FISH_TRAP_TICK_INTERVAL = 0.5f;
    private float FISH_TRAP_DETECTION_RANGE = 2f;

    void Update()
    {
        if (!IsOwner) { return; }

        if (Time.time > nextTickTime)
        {
            StartCoroutine(SlowInterval());
        }
    }

    private IEnumerator SlowInterval()
    {
        nextTickTime = Time.time + FISH_TRAP_TICK_INTERVAL;

        foreach (GameObject player in GameManager.Instance.playerPrefabs)
        {
            if (player == parent.gameObject) { continue; }
            if (Vector3.Distance(transform.position, player.transform.position) <= FISH_TRAP_DETECTION_RANGE)
            {
                GameManager.Instance.Stun(player, FISH_TRAP_TICK_INTERVAL);
                GameManager.Instance.DealDamage(parent.gameObject, player, parent.FISH_TRAP_DAMAGE);
            }
        }
        yield return new WaitForSeconds(FISH_TRAP_TICK_INTERVAL);
    }
}
