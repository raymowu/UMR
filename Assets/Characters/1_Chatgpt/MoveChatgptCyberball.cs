using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveChatgptCyberball : NetworkBehaviour
{
    [SerializeField] private float shootForce;
    private Rigidbody rb;
    public GameObject parent;

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
        // Make sure player it collides with isnt itself
        
        Debug.Log(GetComponent<Collider>());
        Debug.Log(other.gameObject);
        Debug.Log(parent);
        Debug.Log(other.gameObject == parent);
        
        if (other.gameObject == parent) { return; }
        Debug.Log("reached");
        DestroyAbility1ServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyAbility1ServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
