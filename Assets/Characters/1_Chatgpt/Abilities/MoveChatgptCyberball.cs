using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveChatgptCyberball : NetworkBehaviour
{
    [SerializeField] private float shootForce;
    public ChatgptAbilities parent;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) { return; }
        // Move projectile forward in straight line based on the player facing direction
        rb.velocity = rb.transform.forward * shootForce;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) { return; }
        GameManager.Instance.DealDamage(other.gameObject, parent.GetComponent<PlayerPrefab>().Damage);
        GameManager.Instance.IncreaseDamage(parent.gameObject, 1);
        DestroyCyberballHitServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyCyberballHitServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
