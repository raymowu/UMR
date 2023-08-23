using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BillboardCanvas : MonoBehaviour
{
    Transform cameraTransform;
    void Start()
    {   
        cameraTransform = Camera.main.transform;
    }

    // honestly dont even know why this works in multiplayer
    void LateUpdate()
    {
        cameraTransform = Camera.main.transform;
        if (cameraTransform == null) { return; }
        transform.LookAt(transform.position + cameraTransform.rotation * -Vector3.forward, cameraTransform.rotation * Vector3.up);
    }
}
