using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandlePoliceCarCollision : NetworkBehaviour
{
    public FBIAbilities parent;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) { return; }
        GameManager.Instance.DealDamage(parent.gameObject, other.gameObject, parent.GetComponent<PlayerPrefab>().Damage + parent.HIGH_SPEED_CHASE_COLLISION_DAMAGE);
        GameManager.Instance.RemoveSlowsAndSpeeds(parent.gameObject);
        parent.TogglePoliceCarServerRpc(false);
    }
}
