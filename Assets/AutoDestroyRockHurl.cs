using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroyRockHurl : NetworkBehaviour
{
    public float delayBeforeDestroy = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyCyberball());
    }

    IEnumerator DestroyCyberball()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyAbility1ServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyAbility1ServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }



}
