using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ClientData
{
    public ulong clientId;
    public int characterId = -1;

    public ClientData(ulong clientId)
    {
        this.clientId = clientId;
    }
}
