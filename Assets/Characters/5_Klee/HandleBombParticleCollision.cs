using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class HandleBombParticleCollision : NetworkBehaviour
{

    public KleeAbilities parent;


    void OnParticleCollision(GameObject other)
    {
        if(!IsOwner)
        {
            return;
        }
        GameManager.Instance.DealDamage(other, parent.JUMPTY_DUMPTY_DAMAGE);
        GameManager.Instance.Speed(other, 0.5f, parent.JUMPTY_DUMPTY_SLOW_DURATION);
        // DestroyBombServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyBombServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
