using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveChatgptCyberball : NetworkBehaviour
{
    [SerializeField] private float shootForce;
    public ChatgptAbilities parent;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Move projectile forward based on the player facing direction
        rb.velocity = rb.transform.forward * shootForce;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) { return;  }
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
