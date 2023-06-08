using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveChatgptCyberball : NetworkBehaviour
{
    [SerializeField] private float shootForce;
    private Rigidbody rb;
    private const float CYBERBALLSLOW = 0.5f;

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
        GameManager.Instance.TakeDamage(other.gameObject, GetComponent<PlayerPrefab>().Damage);
        GameManager.Instance.Slow(other.gameObject, CYBERBALLSLOW);
/*        StartCoroutine(Unslow());*/
        DestroyAbility1ServerRpc();
    }

/*    IEnumerator Unslow()
    {
        yield return new WaitForSeconds(delayBeforeDestroy);
        DestroyCyberballServerRpc();
    }*/

    [ServerRpc(RequireOwnership = false)]
    public void DestroyAbility1ServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
