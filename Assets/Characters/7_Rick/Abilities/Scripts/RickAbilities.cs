
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class RickAbilities : CharacterAbilities
{
    [SerializeField] private GameObject Morty;

    public float PORTAL_CAST_RANGE = 5f;
    public bool entrancePortalExists = false;
    public bool exitPortalExists = false;
    public GameObject entrancePortal;
    public GameObject exitPortal;

    //[Header("Schwifty Beam")]

    [Header("Interdimensional Leap")]
    [SerializeField] private GameObject entrancePortalPrefab;
    [SerializeField] public GameObject exitPortalPrefab;

    //[Header("Ability 3")]


    //[Header("Ability 4")]

    protected new void Start()
    {
        base.Start();
        SummonMortyServerRpc(gameObject.transform.position);
    }

    [ServerRpc]
    private void SummonMortyServerRpc(Vector3 pos)
    {
        GameObject go = Instantiate(Morty, new Vector3(pos.x, pos.y, pos.z), new Quaternion(0, 0, 0, 1));
        go.GetComponent<MortyAI>().parent = gameObject;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability1Canvas()
    {
        LinearProjectileCanvas(ability1IndicatorCanvas);
    }

    protected override void Ability2Canvas()
    {
        SummonThingCanvas(ability2IndicatorCanvas, PORTAL_CAST_RANGE);
    }

    protected override void Ability3Canvas()
    {
        // TODO
    }

    protected override void Ability4Canvas()
    {
        // TODO
    }

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ability1Cooldown,
                    ref currentAbility1Cooldown, "CastSchwiftyBeam", () =>
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            GameManager.Instance.Root(gameObject, 0.5f);
                        }
                    });

    }

    protected override void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ability2Cooldown,
            ref currentAbility2Cooldown, "CastInterdimensionalLeap", () =>
            {
                // cast entrance portal
                if (!entrancePortalExists)
                {
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        playerMovement.StopMovement();
                        playerMovement.Rotate(hit.point);
                        float distance = Vector3.Distance(hit.point, transform.position);
                        Vector3 portalPosition = hit.point;
                        CastEntrancePortalServerRpc(new Vector3(portalPosition.x, 0f, portalPosition.z),
                        Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) - transform.position));
                    }
                    entrancePortalExists = true;
                }
                // exit portal
                else
                {
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        playerMovement.StopMovement();
                        playerMovement.Rotate(hit.point);
                        float distance = Vector3.Distance(hit.point, transform.position);
                        Vector3 portalPosition = hit.point;
                        CastExitPortalServerRpc(new Vector3(portalPosition.x, 0f, portalPosition.z),
                        Quaternion.LookRotation(new Vector3(hit.point.x, 0f, hit.point.z) - transform.position));
                    }
                    exitPortalExists = true;
                    isAbility2Cooldown = true;
                    currentAbility2Cooldown = ability2Cooldown;
                }
            });
    }

    [ServerRpc]
    private void CastEntrancePortalServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(entrancePortalPrefab, new Vector3(pos.x, pos.y, pos.z), rot);
        entrancePortal = go;
        entrancePortalExists = true;
        go.GetComponent<HandleRickPortalCollision>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
        UpdateEntrancePortalStatusClientRpc();
    }

    [ClientRpc]
    private void UpdateEntrancePortalStatusClientRpc()
    {
        entrancePortalExists = true;
    }

    [ServerRpc]
    private void CastExitPortalServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(exitPortalPrefab, new Vector3(pos.x, pos.y, pos.z), rot);
        exitPortal = go;
        exitPortalExists = true;
        go.GetComponent<AutoDestroyPortals>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
        UpdateExitPortalStatusClientRpc();
    }

    [ClientRpc]
    private void UpdateExitPortalStatusClientRpc()
    {
        exitPortalExists = true;
    }

    protected override void Ability3Input()
    {

    }

    protected override void Ability4Input()
    {

    }
}