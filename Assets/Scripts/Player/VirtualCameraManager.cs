using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;

public class VirtualCameraManager : MonoBehaviour
{
    public GameObject[] players;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<CinemachineVirtualCamera>().Follow = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
