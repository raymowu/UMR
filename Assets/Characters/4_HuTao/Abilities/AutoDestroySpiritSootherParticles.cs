using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroySpiritSootherParticles : NetworkBehaviour
{
    public float delayBeforeDestroy = 1f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroySpiritSootherParticles());
    }

    IEnumerator DestroySpiritSootherParticles()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroySpiritSootherParticlesServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroySpiritSootherParticlesServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
