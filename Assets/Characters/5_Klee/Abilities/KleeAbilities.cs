
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class KleeAbilities: CharacterAbilities
{
    [SerializeField] private Transform fishTrapTransform;

    public float FISH_TRAP_RANGE = 5f;
    public float FISH_TRAP_DAMAGE = 10f;

    [Header("Jumpy Dumpty")]
    public GameObject ability1Projectile;
    public float JUMPY_DUMPTY_DAMAGE = 10f;
    public float JUMPY_DUMPTY_SLOW_AMOUNT = 0.8f;
    public float JUMPY_DUMPTY_SLOW_DURATION = 5f;

    [Header("Fish Trap")]
    public GameObject fishTrap;

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
        SummonProjectileCanvas(ability2IndicatorCanvas, FISH_TRAP_RANGE);
    }

    protected override void Ability3Canvas()
    {
        // TODO
    }

    protected override void Ability4Canvas()
    {
        LinearProjectileCanvas(ability4IndicatorCanvas);
    }

protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ref ability1Cooldown,
                    ref currentAbility1Cooldown, () => {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            GameManager.Instance.Root(gameObject, 0.5f);
                            CastAbility1ServerRpc(new Vector3(hit.point.x, 0f, hit.point.z), Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                        }
                    });
    }

    [ServerRpc]
    private void CastAbility1ServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(ability1Projectile, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveJumpyDumpty>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ref ability2Cooldown,
            ref currentAbility2Cooldown, () => {
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

    [ServerRpc]
    private void CastFishTrapServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(fishTrap, new Vector3(pos.x, pos.y, pos.z), rot);
        go.GetComponent<HandleFishTrapCollision>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }


    protected override void Ability3Input()
    {
    }

    public void SummonAutoProjectile(GameObject parent, GameObject target)
    {
        SummonAutoProjectileServerRpc(parent.GetComponent<NetworkObject>().OwnerClientId,
            target.GetComponent<NetworkObject>().OwnerClientId);
    }

    [ServerRpc]
    private void SummonAutoProjectileServerRpc(ulong parentId, ulong targetId)
    {
        NetworkList<PlayerStats> players = GameManager.Instance.players;
        GameObject[] playerPrefabs = GameManager.Instance.playerPrefabs;
        GameObject parent = playerPrefabs[0];
        GameObject target = playerPrefabs[0];

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != parentId) { continue; }
            parent = playerPrefabs[i];
            break;
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != targetId) { continue; }
            target = playerPrefabs[i];
            break;
        }

        GameObject go = Instantiate(sparkPrefab, shootTransform.position, Quaternion.identity);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), parent.GetComponent<Collider>());
        go.GetComponent<MoveRangedAuto>().parent = parent;
        go.GetComponent<MoveRangedAuto>().target = target;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability4Input()
    {
        InputHelper(ability4Key, ref isAbility4Cooldown, ability4IndicatorCanvas, ref ability4Cooldown,
                  ref currentAbility4Cooldown, () => {
                      if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                      {
                          playerMovement.StopMovement();
                          playerMovement.Rotate(hit.point);
                          CastAbility4ServerRpc(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                      }
                  });
    }

    [ServerRpc]
    private void CastAbility4ServerRpc(Quaternion rot)
    {
        GameObject go = Instantiate(kingDodoco, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveJumpyDumpty>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }
}
