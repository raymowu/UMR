using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MobSpawner1 : NetworkBehaviour
{
    [SerializeField] private MobDatabase mobDatabase;
    public const int MOBS_PER_PHASE = 23;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        var mob = mobDatabase.GetMobById(1);
        if (mob != null)
        {
            //var spawnPos = GameManager.Instance.mobSpawnPoints[0];
            var spawnPos = new Vector3(0f, 0f, 0f);
            var mobInstance = Instantiate(mob.GameplayPrefab, spawnPos, Quaternion.identity);
            mobInstance.GetComponent<EnvMeleeMobAI>().spawnPoint = spawnPos;
            mobInstance.GetComponent<NetworkObject>().Spawn();
        }

    }
}
