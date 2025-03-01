using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed : Effect
{
    public Speed(ulong clientId_, float amount_ = 0, float duration_ = 0)
    {
        clientId = clientId_;
        amount = amount_;
        duration = duration_;
    }
}
