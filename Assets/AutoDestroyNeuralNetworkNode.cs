using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AutoDestroyNeuralNetworkNode : NetworkBehaviour
{
    public float delayBeforeDestroy = 10f;

    void Start()
    {
        StartCoroutine(DestroyNeuralNetworkNode());
    }

    IEnumerator DestroyNeuralNetworkNode()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyNeuralNetworkNodeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyNeuralNetworkNodeServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
