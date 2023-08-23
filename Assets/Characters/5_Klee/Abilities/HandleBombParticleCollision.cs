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
        GameManager.Instance.DealDamage(parent.gameObject, other, parent.JUMPY_DUMPTY_DAMAGE);
        GameManager.Instance.Slow(other, parent.JUMPY_DUMPTY_SLOW_AMOUNT, parent.JUMPY_DUMPTY_SLOW_DURATION);
    }
}
