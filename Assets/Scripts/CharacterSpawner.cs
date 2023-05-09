using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return;  }

        foreach(var client in HostManager.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                // TODO: set spawnpoints here
                var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));

                // Makes sure client it belongs to is the owner of that object
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }
    }
}
