using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroyHackDust : NetworkBehaviour
{
    public float delayBeforeDestroy = 0.5f;

    void Start()
    {
        StartCoroutine(DestroyHackDust());
    }

    IEnumerator DestroyHackDust()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyHackDustServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyHackDustServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
