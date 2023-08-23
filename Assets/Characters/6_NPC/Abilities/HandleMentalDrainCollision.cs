using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandleMentalDrainCollision : NetworkBehaviour
{
    public NPCAbilities parent;
    private float nextTickTime = 0f;

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
        nextTickTime = Time.time + parent.MENTAL_DRAIN_TICK_INTERVAL;

        foreach (GameObject player in GameManager.Instance.playerPrefabs)
        {
            if (player == parent.gameObject) { continue; }
            if (Vector3.Distance(transform.position, player.transform.position) <= parent.MENTAL_DRAIN_RADIUS)
            {
                Debug.Log("here");
                GameManager.Instance.Slow(player, parent.MENTAL_DRAIN_SLOW_AMOUNT, parent.MENTAL_DRAIN_SLOW_DURATION);
                GameManager.Instance.DealDamage(parent.gameObject, player, parent.MENTAL_DRAIN_DAMAGE);
            }
        }
        yield return new WaitForSeconds(parent.MENTAL_DRAIN_TICK_INTERVAL);
    }
}
