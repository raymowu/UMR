using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Effect : NetworkBehaviour
{
    public ulong clientId;
    public float amount;
    public float duration;
    public GameManager.TargetType targetType;
}
