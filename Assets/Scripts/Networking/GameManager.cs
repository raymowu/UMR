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
    public NetworkList<PlayerStats> players;

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
    public void DealDamage(GameObject target, float damage)
    {
        DealDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DealDamageServerRpc(ulong clientId, float damage)
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
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed
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
                    players[i].Damage,
                    players[i].IsSilenced,
                    players[i].IsDisarmed
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
                    players[i].Damage,
                    players[i].IsSilenced,
                    players[i].IsDisarmed
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
                players[i].Damage + damage,
                players[i].IsSilenced,
                players[i].IsDisarmed
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
                players[i].Damage - damage,
                players[i].IsSilenced,
                players[i].IsDisarmed
                );
        }
    }

    // Take in decimal of slow %, i.e. 50% slow = 0.5 or a speed %, 50% speed = 1.5
    public void Speed(GameObject target, float speedAmount, float speedDuration)
    {
        SpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, speedAmount);
        StartCoroutine(Unspeed(target, speedAmount, speedDuration));
    }

    IEnumerator Unspeed(GameObject target, float speedAmount, float speedDuration)
    {
        yield return new WaitForSeconds(speedDuration);
        Speed(target, 1 / speedAmount);
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
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed
                );
        }
    }

    public void Root(GameObject target, float rootDuration)
    {
        Speed(target, .01f, rootDuration);
    }

    public void Silence(GameObject target, float silenceDuration)
    {
        SilenceServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
        StartCoroutine(Unsilence(target, silenceDuration));
    }

    IEnumerator Unsilence(GameObject target, float silenceDuration)
    {
        yield return new WaitForSeconds(silenceDuration);
        UnsilenceServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SilenceServerRpc(ulong clientId)
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
                players[i].Damage,
                true,
                players[i].IsDisarmed
                );
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnsilenceServerRpc(ulong clientId)
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
                players[i].Damage,
                false,
                players[i].IsDisarmed
                );
        }
    }

    public void Disarm(GameObject target, float silenceDuration)
    {
        DisarmServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
        StartCoroutine(Undisarm(target, silenceDuration));
    }

    IEnumerator Undisarm(GameObject target, float silenceDuration)
    {
        yield return new WaitForSeconds(silenceDuration);
        UndisarmServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DisarmServerRpc(ulong clientId)
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
                players[i].Damage,
                players[i].IsSilenced,
                true
                );
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UndisarmServerRpc(ulong clientId)
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
                players[i].Damage,
                players[i].IsSilenced,
                false
                );
        }
    }
    public void Stun(GameObject target, float stunDuration)
    {
        DisarmServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
        StartCoroutine(Undisarm(target, stunDuration));
        SilenceServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
        StartCoroutine(Unsilence(target, stunDuration));
        Root(target, stunDuration);
    }

    public void Knockup(GameObject target, float knockupDuration)
    {
        Stun(target, knockupDuration);
        KnockupServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, knockupDuration);
    }

    [ServerRpc(RequireOwnership = false)]
    private void KnockupServerRpc(ulong clientId, float knockupDuration)
    {
        KnockupClientRpc(clientId, knockupDuration);
    }

    [ClientRpc]
    private void KnockupClientRpc(ulong clientId, float knockupDuration)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            StartCoroutine(UpForce(playerPrefabs[i], knockupDuration));
            break;
        }
    }

    IEnumerator UpForce(GameObject player, float knockupDuration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + knockupDuration / 2)
        {
            player.GetComponent<PlayerMovement>().StopMovement();
            player.GetComponent<CharacterController>().Move(Vector3.up * 10 * Time.deltaTime);
            yield return null;
        }
        StartCoroutine(Gravity(player, knockupDuration));
    }
    IEnumerator Gravity(GameObject player, float knockupDuration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + knockupDuration / 2)
        {
            player.GetComponent<PlayerMovement>().StopMovement();
            player.GetComponent<CharacterController>().Move(Vector3.down * 10 * Time.deltaTime);
            yield return null;
        }
    }
}
