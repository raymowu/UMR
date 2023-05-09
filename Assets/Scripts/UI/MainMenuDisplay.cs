using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MainMenuDisplay : MonoBehaviour
{

    public void StartHost()
    {
        HostManager.Instance.StartHost();
    }

    public void StartClient()
    {
        ClientManager.Instance.StartClient();
    }
}
