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

        // outer mobs
        for (int i = 1; i < 9; i++)
        {
            int j = 0;
            var mob = mobDatabase.GetMobById(i);
            if (mob != null)
            {
                var spawnPos = GameManager.Instance.mobSpawnPoints[j++];
                Debug.Log(spawnPos);

                var mobInstance = Instantiate(mob.GameplayPrefab, spawnPos, Quaternion.identity);
                mobInstance.GetComponent<EnvMeleeMobAI>().spawnPoint = spawnPos;
                mobInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
