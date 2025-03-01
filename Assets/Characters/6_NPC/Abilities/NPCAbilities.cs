using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class NPCAbilities : CharacterAbilities
{
    [SerializeField] private Transform multiplyShootTransform;
    [SerializeField] private Transform wallShootTransform;
    [SerializeField] private Transform mentalDrainShootTransform;

    [Header("MULTIPLY")]
    [SerializeField] private GameObject NPCClone;
    public float MULTIPLY_RANGE = 5f;

    [Header("NPC WALL")]
    [SerializeField] private GameObject NPCWall;

    [Header("MENTAL DRAIN")]
    [SerializeField] private GameObject MentalDrainParticles;
    public float MENTAL_DRAIN_RANGE = 5f;
    public float MENTAL_DRAIN_DURATION = 5f;
    public float MENTAL_DRAIN_RADIUS = 2.5f;
    public float MENTAL_DRAIN_SLOW_AMOUNT = 0.5f;
    public float MENTAL_DRAIN_SLOW_DURATION = 0.5f;
    public float MENTAL_DRAIN_DAMAGE = 10f;
    public float MENTAL_DRAIN_TICK_INTERVAL = 0.5f;

    // [Header("NPC ARMY")]

    protected override void Ability1Canvas()
    {
        SummonThingCanvas(ability1IndicatorCanvas, MULTIPLY_RANGE);
    }

    protected override void Ability2Canvas()
    {
        LinearProjectileCanvas(ability2IndicatorCanvas);
    }

    protected override void Ability3Canvas()
    {
        SummonThingCanvas(ability3IndicatorCanvas, MENTAL_DRAIN_RADIUS);
    }

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ability1Cooldown,
                    ref currentAbility1Cooldown, "CastMultiply", () => {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            float distance = Vector3.Distance(hit.point, transform.position);
                            Vector3 spawnClonePosition = distance <= MULTIPLY_RANGE ? hit.point : multiplyShootTransform.position;
                            SummonNPCCloneServerRpc(new Vector3(spawnClonePosition.x, 0f, spawnClonePosition.z),
                            Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) - transform.position));
                        }
                    });
    }

    [ServerRpc]
    private void SummonNPCCloneServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(NPCClone, new Vector3(pos.x, pos.y, pos.z), rot);
        go.GetComponent<MeleeMobAI>().parent = gameObject;
        go.GetComponent<MeleeMobCombat>().parent = gameObject;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ability2Cooldown,
                    ref currentAbility2Cooldown, "CastNPCWall", () => {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            SummonNPCWallServerRpc(hit.point, Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) -
                                new Vector3(transform.position.x, 0f, transform.position.z)));
                        }
                    });
    }

    [ServerRpc]
    private void SummonNPCWallServerRpc(Vector3 pos, Quaternion rot)
    {
        // Need to rotate player in server RPC so the position is correct realtime value for server
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(NPCWall, new Vector3(wallShootTransform.position.x, 0f, wallShootTransform.position.z), rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability3Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3IndicatorCanvas, ability3Cooldown,
                     ref currentAbility3Cooldown, "CastMentalDrain", () => {
                         if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                         {
                             playerMovement.StopMovement();
                             playerMovement.Rotate(hit.point);
                             float distance = Vector3.Distance(hit.point, transform.position);
                             Vector3 spawnMentalDrainPosition = distance <= MENTAL_DRAIN_RANGE ? hit.point : mentalDrainShootTransform.position;
                             SummonMentalDrainParticlesServerRpc(new Vector3(spawnMentalDrainPosition.x, .1f, spawnMentalDrainPosition.z),
                                 Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) - transform.position));
                         }
                     });
    }

    [ServerRpc]
    private void SummonMentalDrainParticlesServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(hit.point);
        GameObject go = Instantiate(MentalDrainParticles, pos, rot);
        go.GetComponent<HandleMentalDrainCollision>().parent = this;
        go.GetComponent<AutoDestroyGameObject>().delayBeforeDestroy = MENTAL_DRAIN_DURATION;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability4Input()
    {
        InputHelper(ability4Key, ref isAbility4Cooldown, ability4IndicatorCanvas, ability4Cooldown,
            ref currentAbility4Cooldown, "CastNPCArmy", () => {
                    playerMovement.StopMovement();
                    playerMovement.Rotate(hit.point);
                    SummonNPCCloneServerRpc(new Vector3(transform.position.x + 2f, 0f, transform.position.z),
                        transform.rotation);
                    SummonNPCCloneServerRpc(new Vector3(transform.position.x - 2f, 0f, transform.position.z),
                        transform.rotation);
                    SummonNPCCloneServerRpc(new Vector3(transform.position.x, 0f, transform.position.z + 2f),
                        transform.rotation);
                    SummonNPCCloneServerRpc(new Vector3(transform.position.x, 0f, transform.position.z - 2f),
                        transform.rotation);
            });
    }
}
