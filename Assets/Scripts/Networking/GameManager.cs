using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    //TODO UPDATE PLAYER DISPLAY SUCH AS HEALTH BAR VALUE HERE
    //[SerializeField] private CharacterDatabase characterDatabase;
    //[SerializeField] private Image characterIconImage;
    //[SerializeField] private TMP_Text playerNameText;
    //[SerializeField] private TMP_Text characterNameText;
    [Header("References")]
    // sync prefabs TODO: find a way to couple the prefab with stats
    [SerializeField] public GameObject[] playerPrefabs;
    [SerializeField] public GameObject[] mobPrefabs;
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private MobDatabase mobDatabase;
    [SerializeField] private ParticleDatabase particleDatabase;
    [SerializeField] public PlayerScoreboardCard[] playerScoreboardCards;

    public int gamePhase = 1;
    public Vector3[] spawnPoints = { new Vector3 { x = -42.6f, y = 0f, z = -21.5f },
                        new Vector3 { x = 44.5f, y = 0f, z = 24.5f},
                        new Vector3 { x = -42f, y = 0f, z = 23.8f},
                        new Vector3 { x = 44.5f, y = 0f, z = -21.5f},
                        new Vector3 { x = 0, y = 0f, z = 51},
                        new Vector3 { x = 0, y = 0f, z = -50}};

    public Vector3[] mobSpawnPoints = { new Vector3 { x = 15f, y = 0f, z = -41f },
                        new Vector3 { x = 21f, y = 0f, z = -32f},
                        new Vector3 { x = 34, y = 0f, z = -31f},
                        new Vector3 { x = 44.5f, y = 0f, z = -21.5f} };

    public static GameManager Instance { get; private set; }

    // sync stats
    public NetworkList<PlayerStats> players;
    public NetworkList<MobStats> mobs;

    private NetworkList<MovementSpeedBuffDebuff> movementSpeedTracker;

    private void Awake()
    {
        players = new NetworkList<PlayerStats>();
        mobs = new NetworkList<MobStats>();
        movementSpeedTracker = new NetworkList<MovementSpeedBuffDebuff>();
        Instance = this;
    }

    //TODO: handle client disconnect and remove from players and playerPrefabs list
    public override void OnNetworkSpawn()
    {
        // Players and mobs are already spawned by their spawner gameobjects?
        playerPrefabs = GameObject.FindGameObjectsWithTag("Player");
        mobPrefabs = GameObject.FindGameObjectsWithTag("Mob");

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // Initialize players network list
                // (only stats that the character database needs to know are necessary on initialization so NOT game manager stats etc isDead, silenced, etc)
                players.Add(new PlayerStats(client.ClientId, HostManager.Instance.ClientData[client.ClientId].characterId,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).MaxHealth,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).Health,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).AttackSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).MovementSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).CurrentMovementSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).Damage));
            }

            // Initialize mobs network list for phase 1
            foreach (GameObject mob in mobPrefabs)
            {
                Debug.Log("mob" + mob);
                Debug.Log("mob network id: " + mob.GetComponent<NetworkObject>().NetworkObjectId);
                Debug.Log(mob.GetComponent<MobPrefab>().MobId);
                Debug.Log(mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).MaxHealth);
                Debug.Log(mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).Health);
                Debug.Log(mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).AttackSpeed);
                Debug.Log(mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).MovementSpeed);
                Debug.Log(mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).CurrentMovementSpeed);
                Debug.Log(mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).Damage);

                mobs.Add(new MobStats(mob.GetComponent<NetworkObject>().NetworkObjectId,
                    mob.GetComponent<MobPrefab>().MobId,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).MaxHealth,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).Health,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).AttackSpeed,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).MovementSpeed,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).CurrentMovementSpeed,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).Damage));
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

            for (int i = 0; i < mobs.Count; i++)
            {
                mobPrefabs[i].GetComponent<MobPrefab>().UpdateMobStats(mobs[i]);
                mobPrefabs[i].GetComponent<NavMeshAgent>().speed = mobs[i].CurrentMovementSpeed;
            }

            for (int i = 0; i < playerScoreboardCards.Length; i++)
            {
                // Check if there are enough players (only go through existing players)
                if (players.Count > i)
                {
                    playerScoreboardCards[i].UpdateDisplay(players[i]);
                }
                else
                {
                    playerScoreboardCards[i].DisableDisplay();
                }
            }

            players.OnListChanged += HandlePlayersStatsChanged;
            mobs.OnListChanged += HandleMobsStatsChanged;

        }
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStatsChanged;
            mobs.OnListChanged -= HandleMobsStatsChanged;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private GameObject[] RemoveIndices(GameObject[] IndicesArray, int RemoveAt)
    {
        GameObject[] newIndicesArray = new GameObject[IndicesArray.Length - 1];

        int i = 0;
        int j = 0;
        while (i < IndicesArray.Length)
        {
            if (i != RemoveAt)
            {
                newIndicesArray[j] = IndicesArray[i];
                j++;
            }

            i++;
        }

        return newIndicesArray;
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
                playerPrefabs = RemoveIndices(playerPrefabs, i);
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

        // Updates scoreboard Display
        for (int i = 0; i < playerScoreboardCards.Length; i++)
        {
            // Check if there are enough players (only go through existing players)
            if (players.Count > i)
            {
                playerScoreboardCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                playerScoreboardCards[i].DisableDisplay();
            }
        }
    }

    private void HandleMobsStatsChanged(NetworkListEvent<MobStats> changeEvent)
    {
        for (int i = 0; i < mobs.Count; i++)
        {
            mobPrefabs[i].GetComponent<MobPrefab>().UpdateMobStats(mobs[i]);
            mobPrefabs[i].GetComponent<NavMeshAgent>().speed = mobs[i].CurrentMovementSpeed;
        }
    }

    /*
     * BUFFS AND DEBUFFS MANAGER
     */

    public void DealDamage(GameObject sender, GameObject target, float damage)
    {
        DealDamageServerRpc(sender.GetComponent<NetworkObject>().OwnerClientId, target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DealDamageServerRpc(ulong senderId, ulong clientId, float damage)
    {
        int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            // safety check
            if (playerPrefabs[i].layer == LayerIgnoreRaycast) { return; }
            int deaths = players[i].Health - damage > 0 ? players[i].Deaths : players[i].Deaths + 1;
            bool isDead = players[i].Health - damage > 0 ? false : true;
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
                players[i].IsDisarmed,
                players[i].Kills,
                deaths,
                isDead
                );

            // Handle death
            if (players[i].Health <= 0)
            {
                StartCoroutine(Respawn(clientId, gamePhase == 1 ? 10f : gamePhase == 2 ? 30f : 999f));
                for (int j = 0; j < players.Count; j++)
                {
                    if (players[j].ClientId != senderId) { continue; }
                    playerPrefabs[i].GetComponent<PlayerMovement>().targetEnemy = null;
                    players[j] = new PlayerStats(
                        players[j].ClientId,
                        players[j].CharacterId,
                        players[j].MaxHealth,
                        players[j].Health,
                        players[j].AttackSpeed,
                        players[j].MovementSpeed,
                        players[j].CurrentMovementSpeed,
                        players[j].Damage,
                        players[j].IsSilenced,
                        players[j].IsDisarmed,
                        players[j].Kills + 1,
                        players[j].Deaths,
                        players[j].IsDead
                        );
                }
            }
        }
    }

    private IEnumerator Respawn(ulong clientId, float duration)
    {
        HandleBeforeDeath(playerPrefabs[clientId], duration);
        yield return new WaitForSeconds(duration);
        HandleAfterDeathServerRpc(clientId, false);
    }

    [ServerRpc]
    private void HandleAfterDeathServerRpc(ulong clientId, bool isDead)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
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
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                isDead
                );

            playerPrefabs[i].GetComponent<CharacterController>().enabled = false;
            RespawnPlayerClientRpc(clientId);
            playerPrefabs[i].GetComponent<CharacterController>().enabled = true;
            playerPrefabs[i].GetComponent<PlayerMovement>().StopMovement();
            //safety check
            playerPrefabs[i].GetComponent<PlayerMovement>().targetEnemy = null;
        }
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            playerPrefabs[i].transform.position = spawnPoints[i];
        }
    }

    public void TeleportPlayer(GameObject target, Vector3 pos)
    {
        TeleportPlayerServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, pos);
    }

    [ServerRpc]
    private void TeleportPlayerServerRpc(ulong clientId, Vector3 pos)
    {
        TeleportPlayerClientRpc(clientId, pos);
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong clientId, Vector3 pos)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            playerPrefabs[i].transform.position = pos;
        }
    }

    public void Heal(GameObject target, float amount)
    {
        HealServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealServerRpc(ulong clientId, float amount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            if (players[i].Health + amount >= players[i].MaxHealth)
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
                    players[i].IsDisarmed,
                    players[i].Kills,
                    players[i].Deaths,
                    players[i].IsDead
                    );
            }
            else
            {
                players[i] = new PlayerStats(
                    players[i].ClientId,
                    players[i].CharacterId,
                    players[i].MaxHealth,
                    players[i].Health + amount,
                    players[i].AttackSpeed,
                    players[i].MovementSpeed,
                    players[i].CurrentMovementSpeed,
                    players[i].Damage,
                    players[i].IsSilenced,
                    players[i].IsDisarmed,
                    players[i].Kills,
                    players[i].Deaths,
                    players[i].IsDead
                    );
            }
        }
    }

    public void IncreaseMaxHealth(GameObject target, float amount)
    {
        IncreaseMaxHealthServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseMaxHealthServerRpc(ulong clientId, float amount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth + amount,
                players[i].Health,
                players[i].AttackSpeed,
                players[i].MovementSpeed,
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
                );
        }
    }

    public void DecreaseMaxHealth(GameObject target, float amount)
    {
        DecreaseMaxHealthServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecreaseMaxHealthServerRpc(ulong clientId, float amount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            float newCurrentHealth = players[i].Health >= amount ? amount : players[i].Health;
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth - amount,
                newCurrentHealth,
                players[i].AttackSpeed,
                players[i].MovementSpeed,
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
                );
        }
    }

    public void SetMaxHealth(GameObject target, float amount)
    {
        SetMaxHealthServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMaxHealthServerRpc(ulong clientId, float amount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            float newCurrentHealth = players[i].Health >= amount ? amount : players[i].MaxHealth;
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                amount,
                newCurrentHealth,
                players[i].AttackSpeed,
                players[i].MovementSpeed,
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
                );
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
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
                players[i].IsDisarmed,
                    players[i].Kills,
                    players[i].Deaths,
                    players[i].IsDead
                );
        }
    }

    public void SetDamage(GameObject target, float damage)
    {
        SetDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDamageServerRpc(ulong clientId, float damage)
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
                damage,
                players[i].IsSilenced,
                players[i].IsDisarmed,
                    players[i].Kills,
                    players[i].Deaths,
                    players[i].IsDead
                );
        }
    }

    public void IncreaseAttackSpeed(GameObject target, float amount)
    {
        IncreaseAttackSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseAttackSpeedServerRpc(ulong clientId, float amount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health,
                players[i].AttackSpeed - amount,
                players[i].MovementSpeed,
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
                );
        }
    }
    public void DecreaseAttackSpeed(GameObject target, float amount)
    {
        DecreaseAttackSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecreaseAttackSpeedServerRpc(ulong clientId, float amount)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health,
                players[i].AttackSpeed + amount,
                players[i].MovementSpeed,
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
                );
        }
    }
    public void SetAttackSpeed(GameObject target, float attackSpeed)
    {
        SetAttackSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, attackSpeed);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAttackSpeedServerRpc(ulong clientId, float attackSpeed)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            players[i] = new PlayerStats(
                players[i].ClientId,
                players[i].CharacterId,
                players[i].MaxHealth,
                players[i].Health,
                attackSpeed,
                players[i].MovementSpeed,
                players[i].CurrentMovementSpeed,
                players[i].Damage,
                players[i].IsSilenced,
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
     * Bug if same buff/debuff affects player again before duration of the first buff/debuff ends. So make sure that all buff/debuffs
     * cool down is longer than the duration otherwise it will stack and lead to weird behavior :(
     */
    [ServerRpc(RequireOwnership = false)]
    public void RemoveSlowsAndSpeedsServerRpc(ulong clientId)
    {
        foreach (MovementSpeedBuffDebuff m in movementSpeedTracker)
        {
            if (m.ClientId != clientId) { continue; }
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

    public void RemoveSlowsAndSpeeds(GameObject target)
    {
        RemoveSlowsAndSpeedsServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
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
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
                players[i].IsDisarmed,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
                true,
                players[i].Kills,
                players[i].Deaths,
                players[i].IsDead
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
                false,
                    players[i].Kills,
                    players[i].Deaths,
                    players[i].IsDead
                );
        }
    }

    public void Untargetable(GameObject target, float duration)
    {
        UntargetableServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
        StartCoroutine(Retargetable(target, duration));
    }

    IEnumerator Retargetable(GameObject target, float duration)
    {
        yield return new WaitForSeconds(duration);
        RetargetableServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UntargetableServerRpc(ulong clientId)
    {
        UntargetableClientRpc(clientId);
    }

    [ClientRpc]
    private void UntargetableClientRpc(ulong clientId)
    {
        int LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            playerPrefabs[i].layer = LayerIgnoreRaycast;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RetargetableServerRpc(ulong clientId)
    {
        RetargetableClientRpc(clientId);
    }

    [ClientRpc]
    private void RetargetableClientRpc(ulong clientId)
    {
        int playerLayer = LayerMask.NameToLayer("Player");

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            playerPrefabs[i].layer = playerLayer;
        }
    }

    public void Stun(GameObject target, float stunDuration)
    {
        Disarm(target, stunDuration);
        Silence(target, stunDuration);
        Root(target, stunDuration);
        SummonStunParticles(target, stunDuration);
    }

    public void HandleBeforeDeath(GameObject target, float stunDuration)
    {
        Disarm(target, stunDuration);
        Silence(target, stunDuration);
        Root(target, stunDuration);
        // 5 seconds of invincibility after respawn
        Untargetable(target, stunDuration + 5f);
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

    /*
     * PARTICLE MANAGER
     */

    [ClientRpc]
    private void enableParticleClientRpc(NetworkObjectReference particle)
    {
        particle.TryGet(out NetworkObject go);
        go.gameObject.SetActive(true);
    }

    IEnumerator DestroyParticle(GameObject particle, float duration)
    {
        yield return new WaitForSeconds(duration);
        DestroyParticleServerRpc(particle);
    }

    [ServerRpc]
    private void DestroyParticleServerRpc(NetworkObjectReference particle)
    {
        particle.TryGet(out NetworkObject go);
        go.gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(go.gameObject);
    }

    /*
     * Summons constant glowing particles around player
     */
    public void SummonGlowingParticles(GameObject target, float duration)
    {
        SummonGlowingParticlesServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, duration);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SummonGlowingParticlesServerRpc(ulong clientId, float duration)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            GameObject glowParticles = Instantiate(particleDatabase.GetParticleById(ParticleDatabase.GLOW_PARTICLES), playerPrefabs[i].transform, false);
            glowParticles.GetComponent<NetworkObject>().Spawn();
            glowParticles.gameObject.transform.parent = playerPrefabs[i].transform;
            StartCoroutine(DestroyParticle(glowParticles, duration));
        }
    }


    /*
    * Summons strength boost particles with sword that flashes and then a glow around the player for a duration
    */
    public void SummonStrengthParticles(GameObject target)
    {
        SummonStrengthParticlesServerRpc(target.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SummonStrengthParticlesServerRpc(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            // Strength buff particles (destroy is handled automatically by particlesystem)
            GameObject strengthParticles = Instantiate(particleDatabase.GetParticleById(ParticleDatabase.STRENGTH_PARTICLES), playerPrefabs[i].transform, false);
            strengthParticles.GetComponent<NetworkObject>().Spawn();
        }
    }

    /*
    * Summons star dazed particles to indicate stun above the player for a duration
    */
    public void SummonStunParticles(GameObject target, float duration)
    {
        SummonStunParticlesServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, duration);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SummonStunParticlesServerRpc(ulong clientId, float duration)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != clientId) { continue; }
            GameObject stunParticles = Instantiate(particleDatabase.GetParticleById(ParticleDatabase.STUN_PARTICLES), playerPrefabs[i].transform, false);
            stunParticles.GetComponent<NetworkObject>().Spawn();
            stunParticles.gameObject.transform.parent = playerPrefabs[i].transform;
            StartCoroutine(DestroyParticle(stunParticles, duration));
        }
    }
}
