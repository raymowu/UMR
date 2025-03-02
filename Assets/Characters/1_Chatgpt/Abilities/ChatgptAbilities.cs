using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class ChatgptAbilities : CharacterAbilities
{
    [Header("Neural Surge")]
    [SerializeField] private GameObject ability1Projectile;
    public float CYBERBALL_DAMAGE = 10;

    [Header("Code Snare")]
    [SerializeField] private GameObject ability2Projectile;
    public float CODE_SNARE_DURATION = 2f;
    public float CODE_SNARE_ANGLE = 130;
    public float CODE_SNARE_RANGE = 4.5f;

    [Header("Overclock")]
    public float OVERCLOCK_DURATION = 2f;

    [Header("Singularity Protocol")]
    [SerializeField] private GameObject ability4Projectile;

    protected override void Ability1Canvas()
    {
        LinearProjectileCanvas(ability1IndicatorCanvas);
    }

    protected override void Ability2Canvas()
    {
        LinearProjectileCanvas(ability2IndicatorCanvas);
    }

    protected override void Ability4Canvas()
    {
        LinearProjectileCanvas(ability4IndicatorCanvas);
    }

    protected override void Ability1Input()
    {
        InputHelper(ability1Key, ref isAbility1Cooldown, ability1IndicatorCanvas, ability1Cooldown, ref currentAbility1Cooldown,
            "CastNeuralSurge", () =>
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    playerMovement.StopMovement();
                    playerMovement.Rotate(hit.point);
                    CastCyberballServerRpc(hit.point, Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                }
            });
    }

    [ServerRpc]
    private void CastCyberballServerRpc(Vector3 pos, Quaternion rot)
    {
        playerMovement.Rotate(pos);
        GameObject go = Instantiate(ability1Projectile, new Vector3(shootTransform.position.x, shootTransform.position.y, shootTransform.position.z), rot);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveChatgptCyberball>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability2Input()
    {
        InputHelper(ability2Key, ref isAbility2Cooldown, ability2IndicatorCanvas, ability2Cooldown, ref currentAbility2Cooldown,
            "CastCodeSnare", () =>
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    playerMovement.StopMovement();
                    playerMovement.Rotate(hit.point);
                    CastAbility2ServerRpc(Quaternion.LookRotation(new Vector3(hit.point.x, 0, hit.point.z) - transform.position));
                    foreach (GameObject player in GetAllEnemiesInRangeAndWithinAngle(CODE_SNARE_RANGE, CODE_SNARE_ANGLE))
                    {
                        GameManager.Instance.Root(player, CODE_SNARE_DURATION);
                    }
                }
            });
    }

    [ServerRpc]
    private void CastAbility2ServerRpc(Quaternion rot)
    {
        GameObject go = Instantiate(ability2Projectile, shootTransform.position, rot);
        go.GetComponent<NetworkObject>().Spawn();
    }

    protected override void Ability3Input()
    {
        InputHelper(ability3Key, ref isAbility3Cooldown, ability3Cooldown, ref currentAbility3Cooldown,
            "CastOverclock", () =>
            {
                CastAbility3ServerRpc();
            });
    }

    [ServerRpc]
    private void CastAbility3ServerRpc()
    {
        GameManager.Instance.Speed(gameObject, Mathf.Log(stats.Damage, 10), OVERCLOCK_DURATION);
    }

    protected override void Ability4Input()
    {
        InputHelper(ability4Key, ref isAbility4Cooldown, ability4IndicatorCanvas, ability4Cooldown, ref currentAbility4Cooldown,
            "CastSingularityProtocol", () =>
            {
                for (int i = 0; i < (int)stats.Damage / 20; i++)
                {
                    CastAbility4ServerRpc();
                }
            });
    }

    [ServerRpc]
    private void CastAbility4ServerRpc()
    {

        GameObject go = Instantiate(ability4Projectile, new Vector3(shootTransform.position.x + UnityEngine.Random.Range(-5f, 5f), 
            shootTransform.position.y, shootTransform.position.z + UnityEngine.Random.Range(-5f, 5f)), UnityEngine.Random.rotation);
        Physics.IgnoreCollision(go.GetComponent<Collider>(), GetComponent<Collider>());
        go.GetComponent<MoveNeuralNetworkNode>().parent = this;
        go.GetComponent<NetworkObject>().Spawn();
    }
}
