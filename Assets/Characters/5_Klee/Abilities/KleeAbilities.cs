
using UnityEngine;
using Unity.Netcode;

public class KleeAbilities: CharacterAbilities
{
    [Header("Jumpy Dumpty")]
    public GameObject ability1Projectile;
    public float JUMPY_DUMPTY_DAMAGE = 10f;
    public float JUMPY_DUMPTY_SLOW_AMOUNT = 0.8f;
    public float JUMPY_DUMPTY_SLOW_DURATION = 5f;

    [Header("Fish Trap")]
    [SerializeField] private Transform fishTrapTransform;
    public GameObject fishTrap;
    public float FISH_TRAP_RANGE = 5f;
    public float FISH_TRAP_DAMAGE = 10f;

    [Header("Sparks 'n' Splash")]
    public GameObject sparkPrefab;

    [Header("Ability 4")]
    public GameObject kingDodoco;

    protected override void Ability1Canvas()
    {
        LinearProjectileCanvas(ability1IndicatorCanvas);
    }

    protected override void Ability2Canvas()
    {
        SummonThingCanvas(ability2IndicatorCanvas, FISH_TRAP_RANGE);
    }

    protected override void Ability3Canvas()
    {
        // TODO
    }

    protected override void Ability4Canvas()
    {
        LinearProjectileCanvas(ability4IndicatorCanvas);
    }

    /* ABILITY MECHANICS */
    [ServerRpc]
    private void CastJumpyDumptyServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(ability1Projectile, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveJumpyDumpty>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    private void CastFishTrapServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(fishTrap, new Vector3(pos.x, pos.y, pos.z), rot);
        go.GetComponent<HandleFishTrapCollision>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc] 
    private void CastAbility4ServerRpc(Quaternion rot)
    {
        GameObject go = Instantiate(kingDodoco, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveJumpyDumpty>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    /* ABILITY INPUT */

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ability1Cooldown,
                    ref currentAbility1Cooldown, "CastJumpyDumpty", () => {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            GameManager.Instance.Root(gameObject, 0.5f);
                            CastJumpyDumptyServerRpc(new Vector3(hit.point.x, 0f, hit.point.z), Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                        }
                    });
    }

    protected override void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ability2Cooldown,
            ref currentAbility2Cooldown, "CastFishTrap", () => {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    playerMovement.StopMovement();
                    playerMovement.Rotate(hit.point);
                    float distance = Vector3.Distance(hit.point, transform.position);
                    Vector3 fishTrapPosition = distance <= FISH_TRAP_RANGE ? hit.point : fishTrapTransform.position;
                    CastFishTrapServerRpc(new Vector3(fishTrapPosition.x, 0f, fishTrapPosition.z),
                    Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) - transform.position));
                }
            });
    }

    protected override void Ability3Input()
    {
    }

    protected override void Ability4Input()
    {
        InputHelper(ability4Key, ref isAbility4Cooldown, ability4IndicatorCanvas, ability4Cooldown,
                  ref currentAbility4Cooldown, "CastKingDodoco", () => {
                      if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                      {
                          playerMovement.StopMovement();
                          playerMovement.Rotate(hit.point);
                          CastAbility4ServerRpc(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                      }
                  });
    }
}
