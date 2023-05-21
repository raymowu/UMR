using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log(HostManager.Instance.ClientData);
    }
}
