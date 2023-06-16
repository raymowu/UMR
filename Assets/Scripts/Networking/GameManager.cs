using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class GameManager : NetworkBehaviour
{
    //TODO UPDATE PLAYER DISPLAY SUCH AS HEALTH BAR VALUE HERE
    //[SerializeField] private CharacterDatabase characterDatabase;
    //[SerializeField] private Image characterIconImage;
    //[SerializeField] private TMP_Text playerNameText;
    //[SerializeField] private TMP_Text characterNameText;
    [Header("References")]
    // sync player prefabs
    [SerializeField] public GameObject[] playerPrefabs;
    [SerializeField] private CharacterDatabase characterDatabase;

    public static GameManager Instance { get; private set; }

    // Player stats sync
    private NetworkList<PlayerStats> players;

    private void Awake()
    {
        players = new NetworkList<PlayerStats>();
        Instance = this;
    }

    //TODO: handle client disconnect and remove from players and playerPrefabs list
    public override void OnNetworkSpawn()
    {
        playerPrefabs = GameObject.FindGameObjectsWithTag("Player");

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // Initialize players network list
                players.Add(new PlayerStats(client.ClientId, HostManager.Instance.ClientData[client.ClientId].characterId,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).Health,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).MaxHealth,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).AttackSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).MovementSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).Damage));
            }
        }
        if (IsClient)
        {
            // Initializes Display on client first load (until first update change is detected)
            for (int i = 0; i < players.Count; i++)
            {
                playerPrefabs[i].GetComponent<PlayerPrefab>().UpdatePlayerStats(players[i]);
                playerPrefabs[i].GetComponent<NavMeshAgent>().speed = players[i].MovementSpeed;
            }
            players.OnListChanged += HandlePlayersStatsChanged;

        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStatsChanged;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;

        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        // Remove that client from players list
        // RAYMOND NOTE: theres a deallocation error when calling .Count so I just put an if
        // statement that checks if the game has started
        // todo: also remove from playerprefabs

        if (!HostManager.Instance.gameHasStarted)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].ClientId != clientId) { continue; }
                players.RemoveAt(i);
                break;

            }
        }
    }
    private void HandlePlayersStatsChanged(NetworkListEvent<PlayerStats> changeEvent)
    {
        // Updates players Display
        for (int i = 0; i < players.Count; i++)
        {
            playerPrefabs[i].GetComponent<PlayerPrefab>().UpdatePlayerStats(players[i]);
            playerPrefabs[i].GetComponent<NavMeshAgent>().speed = players[i].MovementSpeed;
        }
    }
    public void TakeDamage(GameObject target, float damage)
    {
        TakeDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(ulong clientId, float damage)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health - damage,
                players[i].AttackSpeed,
                players[i].MovementSpeed,
                players[i].Damage
                );
            if (players[i].Health <= 0)
            {
                // Handle death
                Debug.Log("died");
            }
        }
    }

    public void HealDamage(GameObject target, float damage)
    {
        HealDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealDamageServerRpc(ulong clientId, float damage)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            if (players[i].Health + damage >= players[i].MaxHealth)
            {
                players[i] = new PlayerStats(
                    players[i].ClientId,
                    players[i].CharacterId,
                    players[i].MaxHealth,
                    players[i].MaxHealth,
                    players[i].AttackSpeed,
                    players[i].MovementSpeed,
                    players[i].Damage
                    );
            }
            else
            {
                players[i] = new PlayerStats(
                    players[i].ClientId,
                    players[i].CharacterId,
                    players[i].MaxHealth,
                    players[i].Health + damage,
                    players[i].AttackSpeed,
                    players[i].MovementSpeed,
                    players[i].Damage
                    );
            }
        }
    }
    public void IncreaseDamage(GameObject target, float damage)
    {
        IncreaseDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseDamageServerRpc(ulong clientId, float damage)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health,
                players[i].AttackSpeed,
                players[i].MovementSpeed,
                players[i].Damage + damage
                );
        }
    }
    public void DecreaseDamage(GameObject target, float damage)
    {
        DecreaseDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecreaseDamageServerRpc(ulong clientId, float damage)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health,
                players[i].AttackSpeed,
                players[i].MovementSpeed,
                players[i].Damage - damage
                );
        }
    }

    public void Slow(GameObject target, float slowAmount, float slowDuration)
    {
        SlowServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, slowAmount);
        StartCoroutine(Unslow(target, slowAmount, slowDuration));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SlowServerRpc(ulong clientId, float slowAmount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health,
                players[i].AttackSpeed,
                players[i].MovementSpeed * slowAmount,
                players[i].Damage
                );
        }
    }
    IEnumerator Unslow(GameObject target, float slowAmount, float slowDuration)
    {
        yield return new WaitForSeconds(slowDuration);
        Speed(target, 1 / slowAmount);
    }
    public void Speed(GameObject target, float speedAmount)
    {
        SpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, speedAmount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpeedServerRpc(ulong clientId, float speedAmount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health,
                players[i].AttackSpeed,
                players[i].MovementSpeed * speedAmount,
                players[i].Damage
                );
        }
    }
}