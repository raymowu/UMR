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
        GameManager.Instance.DealDamage(parent.gameObject, other, parent.JUMPTY_DUMPTY_DAMAGE);
        GameManager.Instance.Slow(other, parent.JUMPTY_DUMPTY_SLOW_AMOUNT, parent.JUMPTY_DUMPTY_SLOW_DURATION);
    }
}
