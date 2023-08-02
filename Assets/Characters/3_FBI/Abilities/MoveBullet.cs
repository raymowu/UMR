using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveBullet : NetworkBehaviour
{
    [SerializeField] private float shootForce;
    private Rigidbody rb;
    public FBIAbilities parent;

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
        GameManager.Instance.DealDamage(parent.gameObject, other.gameObject, parent.GetComponent<PlayerPrefab>().Damage + parent.MAGNUM_SHOT_DAMAGE);
        DestroyAbility1ServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyAbility1ServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
