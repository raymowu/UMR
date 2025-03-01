using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveDoor : NetworkBehaviour
{
    [SerializeField] private float shootForce = 10f;
    private Rigidbody rb;
    public FBIAbilities parent;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }
        rb.velocity = rb.transform.forward * shootForce;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) { return; }
        GameManager.Instance.DealDamage(parent.gameObject, other.gameObject, parent.GetComponent<PlayerPrefab>().Damage + parent.FBI_OPEN_UP_DAMAGE);
        GameManager.Instance.Speed(other.gameObject, parent.FBI_OPEN_UP_SLOW_AMOUNT, parent.FBI_OPEN_UP_SLOW_DURATION);
        DestroyDoorServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyDoorServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
