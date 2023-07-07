using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroyDoor : NetworkBehaviour
{
    public float delayBeforeDestroy = 0.5f;
    void Start()
    {
        StartCoroutine(DestroyDoor());
    }

    IEnumerator DestroyDoor()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyCyberballMissServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyCyberballMissServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
