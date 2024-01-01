using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MobSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    public const int MOBS_PER_PHASE = 23;



    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        foreach (var client in HostManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                // TODO: set spawnpoints here 
                //var spawnPos = GameManager.Instance.spawnPoints[client.Value.clientId];
                var spawnPos = new Vector3(0f, 0f, 0f);

                // Makes sure client it belongs to is the owner of that object
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }
    }
}
