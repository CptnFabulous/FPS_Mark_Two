using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Vector2LerpEffect : LerpingCosmeticEffect
{
    public Vector2 sizeA;
    public Vector2 sizeB;
    public UnityEvent<Vector2> onLerp;

    public override void SetLerpDirectly(float value)
    {
        Vector2 lerped = Vector2.Lerp(sizeA, sizeB, value);
        onLerp.Invoke(lerped);
    }
}
