using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class VirtualCameraManager : MonoBehaviour
{
    void Start()
    {
        GetComponent<CinemachineVirtualCamera>().Follow = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
    }
}