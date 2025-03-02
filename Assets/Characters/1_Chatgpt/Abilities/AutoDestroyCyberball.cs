using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroyCyberball : NetworkBehaviour
{
    public float delayBeforeDestroy = 0.5f;

    void Start()
    {
        StartCoroutine(DestroyCyberball());
    }

    IEnumerator DestroyCyberball()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyCyberballMissServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyCyberballMissServerRpc()
    {
        GameManager.Instance.IncreaseDamage(GetComponent<MoveChatgptCyberball>().parent.gameObject, -1);
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
