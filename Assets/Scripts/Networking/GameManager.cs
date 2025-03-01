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
    // sync prefabs TODO: find a way to couple the prefab with stats
    public class Player
    {
        public int playerStatsInd = -1;
        public GameObject playerObject;
        public Vector3 spawnPoint;
    }

    public class Mob
    {
        public int mobStatsInd = -1;
        public GameObject mobObject;
        public Vector3 spawnPoint;
    }

    [Header("References")]
    [SerializeField] public GameObject[] playerPrefabsArr;
    [SerializeField] public GameObject[] mobPrefabsArr;
    public Dictionary<ulong, Player> playerPrefabs;
    public Dictionary<ulong, Mob> mobPrefabs;
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private MobDatabase mobDatabase;
    [SerializeField] private ParticleDatabase particleDatabase;
    [SerializeField] public PlayerScoreboardCard[] playerScoreboardCards;

    public int gamePhase = 1;
    public static Vector3[] spawnPoints = { new Vector3 { x = -42.6f, y = 0f, z = -21.5f },
                        new Vector3 { x = 44.5f, y = 0f, z = 24.5f},
                        new Vector3 { x = -42f, y = 0f, z = 23.8f},
                        new Vector3 { x = 44.5f, y = 0f, z = -21.5f},
                        new Vector3 { x = 0, y = 0f, z = 51},
                        new Vector3 { x = 0, y = 0f, z = -50}};

    public static Vector3[] mobSpawnPoints = { new Vector3 { x = 15f, y = 0f, z = -41f },
                        new Vector3 { x = 21f, y = 0f, z = -32f},
                        new Vector3 { x = 34, y = 0f, z = -31f},
                        new Vector3 { x = 44.5f, y = 0f, z = -21.5f} };

    public static GameManager Instance { get; private set; }

    // sync stats
    public static NetworkList<PlayerStats> players;
    public static NetworkList<MobStats> mobs;

    private NetworkList<MovementSpeedBuffDebuff> movementSpeedTracker;

    private void Awake()
    {
        players = new NetworkList<PlayerStats>();
        mobs = new NetworkList<MobStats>();
        movementSpeedTracker = new NetworkList<MovementSpeedBuffDebuff>();
        Instance = this;
    }

    public static Dictionary<ulong, Player> ConvertPlayerArrayToMap(GameObject[] array)
    {
        Dictionary<ulong, Player> resultMap = new Dictionary<ulong, Player>();
        int spawnPointInd = 0;
        foreach (GameObject element in array)
        {
            Player player = new Player();
            ulong clientId = element.GetComponent<NetworkObject>().OwnerClientId;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].ClientId == clientId)
                {
                    player.playerStatsInd = i;
                    break;
                }
            }
            player.playerObject = element;
            player.spawnPoint = spawnPoints[spawnPointInd++];
            resultMap[clientId] = player;
        }
        return resultMap;
    }    
    public static Dictionary<ulong, Mob> ConvertMobArrayToMap(GameObject[] array)
    {
        Dictionary<ulong, Mob> resultMap = new Dictionary<ulong, Mob>();

        foreach (GameObject element in array)
        {
            Mob mob = new Mob();
            ulong mobId = element.GetComponent<NetworkObject>().NetworkObjectId;
            for (int i = 0; i < mobs.Count; i++)
            {
                if (mobs[i].Id == mobId)
                {
                    mob.mobStatsInd = i;
                    break;
                }
            }
            mob.mobObject = element;
            resultMap[mobId] = mob;
        }
        return resultMap;
    }

    //TODO: handle client disconnect and remove from players and playerPrefabs list
    public override void OnNetworkSpawn()
    {
        // Players and mobs are already spawned by their spawner gameobjects?
        playerPrefabsArr = GameObject.FindGameObjectsWithTag("Player");
        mobPrefabsArr = GameObject.FindGameObjectsWithTag("Mob");

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // Initialize players network list
                // (only stats that the character database needs to know are necessary on initialization so NOT game manager stats etc isDead, silenced, etc)
                players.Add(new PlayerStats(
                    client.ClientId, 
                    HostManager.Instance.ClientData[client.ClientId].characterId,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).MaxHealth,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).Health,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).AttackSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).MovementSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).CurrentMovementSpeed,
                    characterDatabase.GetCharacterById(HostManager.Instance.ClientData[client.ClientId].characterId).Damage));
            }

            // Initialize mobs network list for phase 1
            foreach (GameObject mob in mobPrefabsArr)
            {
                mobs.Add(new MobStats(
                    mob.GetComponent<NetworkObject>().NetworkObjectId,
                    mob.GetComponent<MobPrefab>().MobId,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).MaxHealth,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).Health,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).AttackSpeed,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).MovementSpeed,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).CurrentMovementSpeed,
                    mobDatabase.GetMobById(mob.GetComponent<MobPrefab>().MobId).Damage));
            }
        }

        playerPrefabs = ConvertPlayerArrayToMap(playerPrefabsArr);
        mobPrefabs = ConvertMobArrayToMap(mobPrefabsArr);

        if (IsClient)
        {
            // Initializes Display on client first load (until first update change is detected)
            for (int i = 0; i < players.Count; i++)
            {
                playerPrefabsArr[i].GetComponent<PlayerPrefab>().UpdatePlayerStats(players[i]);
                playerPrefabsArr[i].GetComponent<NavMeshAgent>().speed = players[i].CurrentMovementSpeed;
            }

            for (int i = 0; i < mobs.Count; i++)
            {
                mobPrefabsArr[i].GetComponent<MobPrefab>().UpdateMobStats(mobs[i]);
                mobPrefabsArr[i].GetComponent<NavMeshAgent>().speed = mobs[i].CurrentMovementSpeed;
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
                playerPrefabsArr = RemoveIndices(playerPrefabsArr, i);
                playerPrefabs.Remove(players[i].ClientId);
                break;

            }
        }
    }
    private void HandlePlayersStatsChanged(NetworkListEvent<PlayerStats> changeEvent)
    {
        // Updates players Display
        foreach (KeyValuePair<ulong, Player> p in playerPrefabs)
        {
            p.Value.playerObject.GetComponent<PlayerPrefab>().UpdatePlayerStats(players[p.Value.playerStatsInd]);
            p.Value.playerObject.GetComponent<NavMeshAgent>().speed = players[p.Value.playerStatsInd].CurrentMovementSpeed;
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
            mobPrefabsArr[i].GetComponent<MobPrefab>().UpdateMobStats(mobs[i]);
            mobPrefabsArr[i].GetComponent<NavMeshAgent>().speed = mobs[i].CurrentMovementSpeed;
        }
    }

    /*
     * STATS CHANGE MANAGER
     */

    public void DealDamage(GameObject sender, GameObject target, float damage)
    {
        if (target == null) return;
        if (target.CompareTag("Player"))
        {
            DealDamageToPlayerServerRpc(sender.GetComponent<NetworkObject>().OwnerClientId, target.GetComponent<NetworkObject>().OwnerClientId, damage);
        }
        else if (target.CompareTag("Mob"))
        {
            DealDamageToMobServerRpc(sender.GetComponent<NetworkObject>().OwnerClientId, target.GetComponent<NetworkObject>().NetworkObjectId, damage);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DealDamageToPlayerServerRpc(ulong senderId, ulong targetId, float damage)
    {
        PlayerStats target = players[playerPrefabs[targetId].playerStatsInd];
        float newHealth = target.Health - damage;
        target.Health = newHealth;
        target.Deaths = newHealth > 0 ? target.Deaths : target.Deaths + 1;
        target.IsDead = newHealth > 0 ? false : true;
        players[playerPrefabs[targetId].playerStatsInd] = target;

        // Handle death
        if (target.Health <= 0)
        {
            StartCoroutine(RespawnPlayer(targetId, gamePhase == 1 ? 10f : gamePhase == 2 ? 30f : 999f));
            PlayerStats sender = players[playerPrefabs[senderId].playerStatsInd];
            playerPrefabs[senderId].playerObject.GetComponent<PlayerMovement>().targetEnemy = null;
            sender.Kills = sender.Kills + 1;
            players[playerPrefabs[senderId].playerStatsInd] = sender;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DealDamageToMobServerRpc(ulong senderId, ulong mobId, float damage)
    {

        MobStats mob = mobs[mobPrefabs[mobId].mobStatsInd];
        float newHealth = mob.Health - damage;
        mob.Health = newHealth;
        mob.IsDead = newHealth > 0 ? false : true;
        mobs[mobPrefabs[mobId].mobStatsInd] = mob;

        if (mob.Health <= 0)
        {
            StartCoroutine(RespawnMob(mobId, gamePhase == 1 ? 60f : gamePhase == 2 ? 120f : 999f));
        }

    }

    private IEnumerator RespawnPlayer(ulong clientId, float duration)
    {
        HandleBeforeDeath(playerPrefabs[clientId].playerObject, duration);
        yield return new WaitForSeconds(duration);
        HandleAfterPlayerDeathServerRpc(clientId);
    }    
    private IEnumerator RespawnMob(ulong mobId, float duration)
    {
        HandleBeforeDeath(mobPrefabs[mobId].mobObject, duration);
        yield return new WaitForSeconds(duration);
        HandleAfterMobDeathServerRpc(mobId);
    }

    public void HandleBeforeDeath(GameObject target, float stunDuration)
    {
        Disarm(target, stunDuration);
        Silence(target, stunDuration);
        Root(target, stunDuration);
        // 5 seconds of invincibility after respawn
        Untargetable(target, stunDuration + 5f);
    }

    [ServerRpc]
    private void HandleAfterPlayerDeathServerRpc(ulong clientId)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.IsDead = false;
        target.Health = target.MaxHealth;
        players[playerPrefabs[clientId].playerStatsInd] = target;

        playerPrefabs[clientId].playerObject.GetComponent<CharacterController>().enabled = false;
        RespawnPlayerClientRpc(clientId);
        playerPrefabs[clientId].playerObject.GetComponent<CharacterController>().enabled = true;
        playerPrefabs[clientId].playerObject.GetComponent<PlayerMovement>().StopMovement();
        playerPrefabs[clientId].playerObject.GetComponent<PlayerMovement>().targetEnemy = null;
    }
        [ServerRpc]
    private void HandleAfterMobDeathServerRpc(ulong mobId)
    {
        MobStats target = mobs[mobPrefabs[mobId].mobStatsInd];
        target.IsDead = false;
        target.Health = target.MaxHealth;
        mobs[mobPrefabs[mobId].mobStatsInd] = target;

        mobPrefabs[mobId].mobObject.transform.position = mobPrefabs[mobId].spawnPoint;
        mobPrefabs[mobId].mobObject.GetComponent<EnvMeleeMobAI>().StopMovement();
        mobPrefabs[mobId].mobObject.GetComponent<EnvMeleeMobAI>().targetEnemy = null;
    }

    // needed because server does not own player
    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong clientId)
    {
        playerPrefabs[clientId].playerObject.transform.position = playerPrefabs[clientId].spawnPoint;
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
        playerPrefabs[clientId].playerObject.transform.position = pos;
    }

    public void Heal(GameObject target, float amount)
    {
        HealServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        if (target.Health + amount >= target.MaxHealth)
        {
            target.Health = target.MaxHealth;
        }
        else
        {
            target.Health = target.Health + amount;
        }
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    public void IncreaseMaxHealth(GameObject target, float amount)
    {
        IncreaseMaxHealthServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseMaxHealthServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.MaxHealth = target.MaxHealth + amount;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    public void DecreaseMaxHealth(GameObject target, float amount)
    {
        DecreaseMaxHealthServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecreaseMaxHealthServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        float newCurrentMaxHealth = target.MaxHealth - amount;
        target.MaxHealth = newCurrentMaxHealth;
        target.Health = target.Health > newCurrentMaxHealth ? newCurrentMaxHealth : target.Health;
        target.IsDead = target.Health > 0 ? false : true;
        players[playerPrefabs[clientId].playerStatsInd] = target;
        // Handle Death
        if (target.Health <= 0)
        {
            StartCoroutine(RespawnPlayer(clientId, gamePhase == 1 ? 10f : gamePhase == 2 ? 30f : 999f));
        }
    }

    public void SetMaxHealth(GameObject target, float amount)
    {
        SetMaxHealthServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMaxHealthServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        float newCurrentMaxHealth = amount;
        target.MaxHealth = newCurrentMaxHealth;
        target.Health = target.Health > newCurrentMaxHealth ? newCurrentMaxHealth : target.Health;
        target.IsDead = target.Health > 0 ? false : true;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    public void IncreaseDamage(GameObject target, float damage)
    {
        IncreaseDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseDamageServerRpc(ulong clientId, float damage)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.Damage += damage;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }
    public void DecreaseDamage(GameObject target, float damage)
    {
        DecreaseDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecreaseDamageServerRpc(ulong clientId, float damage)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.Damage -= damage;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    public void SetDamage(GameObject target, float damage)
    {
        SetDamageServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDamageServerRpc(ulong clientId, float damage)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.Damage = damage;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    public void IncreaseAttackSpeed(GameObject target, float amount)
    {
        IncreaseAttackSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void IncreaseAttackSpeedServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.AttackSpeed += amount;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }
    public void DecreaseAttackSpeed(GameObject target, float amount)
    {
        DecreaseAttackSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DecreaseAttackSpeedServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.AttackSpeed -= amount;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }
    public void SetAttackSpeed(GameObject target, float attackSpeed)
    {
        SetAttackSpeedServerRpc(target.GetComponent<NetworkObject>().OwnerClientId, attackSpeed);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAttackSpeedServerRpc(ulong clientId, float attackSpeed)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.AttackSpeed = attackSpeed;
        players[playerPrefabs[clientId].playerStatsInd] = target;
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
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.CurrentMovementSpeed = calculateMovementSpeed(clientId);
        players[playerPrefabs[clientId].playerStatsInd] = target;
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
    private void PermReduceMovementSpeedServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.MovementSpeed -= amount;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    /*
     *  Permanently increase movement speed
     *  Takes in an input float of direct value to add to player's current movement speed
     */
    [ServerRpc(RequireOwnership = false)]
    private void PermIncreaseMovementSpeedServerRpc(ulong clientId, float amount)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.MovementSpeed += amount;
        players[playerPrefabs[clientId].playerStatsInd] = target;
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
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.IsSilenced = true;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnsilenceServerRpc(ulong clientId)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.IsSilenced = false;
        players[playerPrefabs[clientId].playerStatsInd] = target;
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
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.IsDisarmed = true;
        players[playerPrefabs[clientId].playerStatsInd] = target;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UndisarmServerRpc(ulong clientId)
    {
        PlayerStats target = players[playerPrefabs[clientId].playerStatsInd];
        target.IsDisarmed = false;
        players[playerPrefabs[clientId].playerStatsInd] = target;
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

        playerPrefabs[clientId].playerObject.layer = LayerIgnoreRaycast;
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
        playerPrefabs[clientId].playerObject.layer = playerLayer;
    }

    public void Stun(GameObject target, float stunDuration)
    {
        Disarm(target, stunDuration);
        Silence(target, stunDuration);
        Root(target, stunDuration);
        SummonStunParticles(target, stunDuration);
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
        StartCoroutine(UpForce(playerPrefabs[clientId].playerObject, knockupDuration));
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
        GameObject glowParticles = Instantiate(particleDatabase.GetParticleById(ParticleDatabase.GLOW_PARTICLES), playerPrefabs[clientId].playerObject.transform, false);
        glowParticles.GetComponent<NetworkObject>().Spawn();
        glowParticles.gameObject.transform.parent = playerPrefabs[clientId].playerObject.transform;
        StartCoroutine(DestroyParticle(glowParticles, duration));
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
            GameObject strengthParticles = Instantiate(particleDatabase.GetParticleById(ParticleDatabase.STRENGTH_PARTICLES), playerPrefabs[clientId].playerObject.transform, false);
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
        GameObject stunParticles = Instantiate(particleDatabase.GetParticleById(ParticleDatabase.STUN_PARTICLES), playerPrefabs[clientId].playerObject.transform, false);
        stunParticles.GetComponent<NetworkObject>().Spawn();
        stunParticles.gameObject.transform.parent = playerPrefabs[clientId].playerObject.transform;
        StartCoroutine(DestroyParticle(stunParticles, duration));
    }
}
