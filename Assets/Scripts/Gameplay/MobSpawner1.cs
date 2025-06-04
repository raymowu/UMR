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
        int mobSpawnInd = 0;
        var goomba = mobDatabase.GetMobById(1);
        var pumpkin = mobDatabase.GetMobById(2);
        var slime = mobDatabase.GetMobById(3);
        var ghost = mobDatabase.GetMobById(4);
        var mask = mobDatabase.GetMobById(5);
        if (goomba != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var spawnPos = GameManager.mobSpawnPoints[mobSpawnInd++];
                var mobInstance = Instantiate(goomba.GameplayPrefab, spawnPos, Quaternion.identity);
                mobInstance.GetComponent<EnvMeleeMobAI>().spawnPoint = spawnPos;
                mobInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
        if (pumpkin != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var spawnPos = GameManager.mobSpawnPoints[mobSpawnInd++];
                var mobInstance = Instantiate(pumpkin.GameplayPrefab, spawnPos, Quaternion.identity);
                mobInstance.GetComponent<EnvMeleeMobAI>().spawnPoint = spawnPos;
                mobInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
        if (slime != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var spawnPos = GameManager.mobSpawnPoints[mobSpawnInd++];
                var mobInstance = Instantiate(slime.GameplayPrefab, spawnPos, Quaternion.identity);
                mobInstance.GetComponent<EnvMeleeMobAI>().spawnPoint = spawnPos;
                mobInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
        if (ghost != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var spawnPos = GameManager.mobSpawnPoints[mobSpawnInd++];
                var mobInstance = Instantiate(ghost.GameplayPrefab, spawnPos, Quaternion.identity);
                mobInstance.GetComponent<EnvMeleeMobAI>().spawnPoint = spawnPos;
                mobInstance.GetComponent<NetworkObject>().Spawn();
            }
        }
        if (mask != null)
        {
            for (int i = 0; i < 3; i++)
            {
                var spawnPos = GameManager.mobSpawnPoints[mobSpawnInd++];
                var mobInstance = Instantiate(mask.GameplayPrefab, spawnPos, Quaternion.identity);
                mobInstance.GetComponent<EnvMeleeMobAI>().spawnPoint = spawnPos;
                mobInstance.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}
