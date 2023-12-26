using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandleRickPortalCollision : NetworkBehaviour
{
    public RickAbilities parent;
    private float PORTAL_DETECTION_RANGE = 2f;

    void Update()
    {
        if (!IsOwner) { return; }
        TeleportPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TeleportPlayerServerRpc()
    {
        if (parent.exitPortalExists)
        {
            foreach (GameObject player in GameManager.Instance.playerPrefabs)
            {
                if (Vector3.Distance(transform.position, player.transform.position) <= PORTAL_DETECTION_RANGE)
                {
                    GameManager.Instance.TeleportPlayer(player, parent.exitPortal.transform.position);
                }
            }
        }
    }
}
