using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroyJumptyDumptyBombParticles : NetworkBehaviour
{
    public float delayBeforeDestroy = 5f;
    void Start()
    {
        StartCoroutine(DestroyJumptyDumptyBombParticles());
    }

    IEnumerator DestroyJumptyDumptyBombParticles()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyJumptyDumptyBombParticlesServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyJumptyDumptyBombParticlesServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
