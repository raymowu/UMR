using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System;

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

    private NetworkList<MovementSpeedBuffDebuff> movementSpeedTracker;

    private void Awake()
    {
        players = new NetworkList<PlayerStats>();
        movementSpeedTracker = new NetworkList<MovementSpeedBuffDebuff>();
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
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).CurrentMovementSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).Damage));
            }
        }
        if (IsClient)
        {
            // Initializes Display on client first load (until first update change is detected)
            for (int i = 0; i < players.Count; i++)
            {
                playerPrefabs[i].GetComponent<PlayerPrefab>().UpdatePlayerStats(players[i]);
                playerPrefabs[i].GetComponent<NavMeshAgent>().speed = players[i].CurrentMovementSpeed;
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
            playerPrefabs[i].GetComponent<NavMeshAgent>().speed = players[i].CurrentMovementSpeed;
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
                players[i].CurrentMovementSpeed,
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
                    players[i].CurrentMovementSpeed,
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
                    players[i].CurrentMovementSpeed,
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
                players[i].CurrentMovementSpeed,
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
                players[i].CurrentMovementSpeed,
                players[i].Damage - damage,
                players[i].IsSilenced,
                players[i].IsDisarmed
                );
        }
    }

    private float calculateMovementSpeed(ulong clientId)
    {
        float ans = 0;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            ans = players[i].MovementSpeed;
        }

        for (int i = 0; i < movementSpeedTracker.Count; i++)
        {
            if (movementSpeedTracker[i].ClientId != clientId) { continue; }
            ans *= movementSpeedTracker[i].Amount;
        }
        return ans;
    }

    [ServerRpc(RequireOwnership = false)]
    private void addToMovementSpeedTrackerServerRpc(ulong clientId, float speedAmount, float speedDuration)
    {
        movementSpeedTracker.Add(new MovementSpeedBuffDebuff(clientId, speedAmount, speedDuration));
    }

    [ServerRpc(RequireOwnership = false)]
    private void removeFromMovementSpeedTrackerServerRpc(ulong clientId, float speedAmount, float speedDuration)
    {
        foreach (MovementSpeedBuffDebuff m in movementSpeedTracker) {
            if (m.ClientId != clientId || 
                m.Amount != speedAmount || m.Duration != speedDuration) { continue; }
            movementSpeedTracker.Remove(m);
            break;
        }
    }

     /* 
     * Take in decimal of speed % i.e. 50% speed = 1.5
     * Client manages the speed buff/debuff, starts their own coroutine to wait out the duration and unbuffs whoever they speed/slowed
     */
    public void Speed(GameObject target, float speedAmount, float speedDuration)
    {
        addToMovementSpeedTrackerServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, speedAmount, speedDuration);
        calcAndSetMovementSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
        StartCoroutine(Unspeed(target, speedAmount, speedDuration));
    }

    IEnumerator Unspeed(GameObject target, float speedAmount, float speedDuration)
    {
        yield return new WaitForSeconds(speedDuration);
        removeFromMovementSpeedTrackerServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, speedAmount, speedDuration);
        calcAndSetMovementSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
    }

    // Take in decimal of slow %, i.e. 50% slow = 0.5
    public void Slow(GameObject target, float slowAmount, float slowDuration)
    {
        addToMovementSpeedTrackerServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, slowAmount, slowDuration);
        calcAndSetMovementSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
        StartCoroutine(Unslow(target, slowAmount, slowDuration));
    }

    IEnumerator Unslow(GameObject target, float slowAmount, float slowDuration)
    {
        yield return new WaitForSeconds(slowDuration);
        removeFromMovementSpeedTrackerServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, slowAmount, slowDuration);
        calcAndSetMovementSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void calcAndSetMovementSpeedServerRpc(ulong clientId)
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
                calculateMovementSpeed(clientId),
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed
                );
        }
    }

    public void PermSpeed(GameObject target, float speedAmount)
    {
        PermIncreaseMovementSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, speedAmount);
    }

    public void PermSlow(GameObject target, float slowAmount)
    {
        PermReduceMovementSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, slowAmount);
    }


    /*
     *  Permanently reduce movement speed
     *  Takes in an input float of direct value to subtract from player's current movement speed
     */
    [ServerRpc(RequireOwnership = false)]
    private void PermReduceMovementSpeedServerRpc(ulong clientId, float slowAmount)
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
                players[i].MovementSpeed - slowAmount,
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed
                );
        }
    }

    /*
     *  Permanently increase movement speed
     *  Takes in an input float of direct value to add to player's current movement speed
     */
    [ServerRpc(RequireOwnership = false)]
    private void PermIncreaseMovementSpeedServerRpc(ulong clientId, float slowAmount)
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
                players[i].MovementSpeed + slowAmount,
                players[i].CurrentMovementSpeed,
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
                players[i].CurrentMovementSpeed,
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
                players[i].CurrentMovementSpeed,
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
                players[i].CurrentMovementSpeed,
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
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                false
                );
        }
    }
    public void Stun(GameObject target, float stunDuration)
    {
        Disarm(target, stunDuration);
        Silence(target, stunDuration);
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
