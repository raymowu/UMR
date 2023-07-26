using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class AutoDestroyGameObject : NetworkBehaviour
{
    public float delayBeforeDestroy;

    void Start()
    {
        StartCoroutine(DestroyGameObject());
    }

    IEnumerator DestroyGameObject()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyGameObjectServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyGameObjectServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
