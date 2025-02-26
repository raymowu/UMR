
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class FBIAbilities : CharacterAbilities
{
    [Header("Magnum Shot ")]
    [SerializeField] private GameObject ability1Projectile;
    public float MAGNUM_SHOT_DAMAGE = 30f;

    [Header("Ability 2")]
    [SerializeField] private GameObject ability2Projectile;
    public float FBI_OPEN_UP_DAMAGE = 10f;
    public float FBI_OPEN_UP_SLOW_AMOUNT = .6f;
    public float FBI_OPEN_UP_SLOW_DURATION = 2f;

    [Header("Ability 3")]
    public float TAZE_RANGE = 5f;
    public float TAZE_DURATION = 1.5f;

    [Header("Ability 4")]
    [SerializeField] private GameObject policeCar;
    public float HIGH_SPEED_CHASE_RANGE = 2f;
    public float HIGH_SPEED_CHASE_ANGLE = 130f;
    public float HIGH_SPEED_CHASE_DURATION = 10f;
    public float HIGH_SPEED_CHASE_SPEED = 3f;
    public float HIGH_SPEED_CHASE_COLLISION_DAMAGE = 60f;
    private bool highSpeedChaseActive = false;

    protected new void Start()
    {
        base.Start();
        policeCar.gameObject.SetActive(false);
    }

    protected override void Ability1Canvas()
    {
        LinearProjectileCanvas(ability1IndicatorCanvas);
    }

    protected override void Ability2Canvas()
    {
        LinearProjectileCanvas(ability2IndicatorCanvas);
    }

    protected override void Ability3Canvas()
    {
        PointAndClickCanvas(ability3IndicatorCanvas);
    }

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ability1Cooldown,
                    ref currentAbility1Cooldown, "CastMagnumShot", () => {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            CastAbility1ServerRpc(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                        }
                    });
    }

    // summon projectile here
    [ServerRpc]
    private void CastAbility1ServerRpc(Quaternion rot)
    {
        GameObject go = Instantiate(ability1Projectile, shootTransform.position, rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveBullet>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ability2Cooldown,
                    ref currentAbility2Cooldown, "CastFBIOpenUp", () => {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            playerMovement.StopMovement();
                            playerMovement.Rotate(hit.point);
                            CastAbility2ServerRpc(hit.point, Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                        }
                    });
    }

    [ServerRpc]
    private void CastAbility2ServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(ability2Projectile, new Vector3(shootTransform.position.x, 0.8f, shootTransform.position.z), rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveDoor>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability3Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3IndicatorCanvas, ability3Cooldown, ref currentAbility3Cooldown,
            "CastTaze", TAZE_RANGE, () =>
            {
                playerMovement.Rotate(hit.point);
                GameManager.Instance.Stun(playerMovement.targetEnemy, TAZE_DURATION);
            });
    }

    protected override void Ability4Input()
    {
        if (highSpeedChaseActive)
        {
            foreach (GameObject player in GetAllPlayersInRangeAndWithinAngle(HIGH_SPEED_CHASE_RANGE, HIGH_SPEED_CHASE_ANGLE))
            {
                highSpeedChaseActive = false;
                GameManager.Instance.DealDamage(gameObject, player.gameObject, GetComponent<PlayerPrefab>().Damage + HIGH_SPEED_CHASE_COLLISION_DAMAGE);
                GameManager.Instance.RemoveSlowsAndSpeeds(gameObject);
                TogglePoliceCarServerRpc(false);
            }
        }

        InputHelper(ability4Key, ref isAbility4Cooldown, ability4Cooldown, ref currentAbility4Cooldown,
            "CastHighSpeedChase", () =>
            {
                highSpeedChaseActive = true;
                TogglePoliceCarServerRpc(true);
                GameManager.Instance.Speed(gameObject, HIGH_SPEED_CHASE_SPEED, HIGH_SPEED_CHASE_DURATION);
                StartCoroutine(DestroyPoliceCar());
            });
    }

    [ServerRpc]
    public void TogglePoliceCarServerRpc(bool active)
    {
        TogglePoliceCarClientRpc(active);
    }

    [ClientRpc]
    private void TogglePoliceCarClientRpc(bool active)
    {
        policeCar.gameObject.SetActive(active);
    }

    IEnumerator DestroyPoliceCar()
    {
        yield return new WaitForSeconds(HIGH_SPEED_CHASE_DURATION);
        GameManager.Instance.RemoveSlowsAndSpeeds(gameObject);
        TogglePoliceCarServerRpc(false);
    }
}
