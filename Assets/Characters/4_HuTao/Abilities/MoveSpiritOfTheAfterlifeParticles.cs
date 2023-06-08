using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveSpiritOfTheAfterlifeParticles : NetworkBehaviour
{
    public HuTaoAbilities parent;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
    [ServerRpc(RequireOwnership = false)]
    public void DestroyAbility2ParticlesServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}

