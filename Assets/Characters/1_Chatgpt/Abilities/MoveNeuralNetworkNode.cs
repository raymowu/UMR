using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveNeuralNetworkNode : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed;
    public ChatgptAbilities parent;

    void Update()
    {
        if (!IsOwner) { return;  }
        transform.RotateAround(parent.transform.position, new Vector3(0, 1, 0), rotationSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) { return; }
        GameManager.Instance.TakeDamage(other.gameObject, parent.GetComponent<PlayerPrefab>().Damage);
        DestroyNeuralNetworkNodeServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyNeuralNetworkNodeServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
