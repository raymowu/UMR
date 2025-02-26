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
            foreach (GameObject player in parent.GetAllPlayersInRange(PORTAL_DETECTION_RANGE, gameObject))
            {
                GameManager.Instance.TeleportPlayer(player, parent.exitPortal.transform.position);
            }
        }
    }
}
