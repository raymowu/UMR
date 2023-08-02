using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveRangedAuto : NetworkBehaviour
{
    public GameObject parent;
    public GameObject target;

    public float velocity = 5;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return;  }
        // TODO: replace if condition with if the target is respawning/dead
/*        if (target == null)
        {
            DestroyRangedAutoServerRpc();
        }*/

        transform.position = Vector3.MoveTowards(transform.position, 
            new Vector3(target.transform.position.x, 1, target.transform.position.z), 
            velocity * Time.deltaTime);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) { return; }
        Debug.Log("trigger");
        GameManager.Instance.DealDamage(parent, other.gameObject, parent.GetComponent<PlayerPrefab>().Damage);
        DestroyRangedAutoServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyRangedAutoServerRpc()
    {
        Debug.Log("destroy");
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
