using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FloatEffect : LerpingCosmeticEffect
{
    public UnityEvent<float> effect;
    public override void SetLerpDirectly(float value) => effect.Invoke(value);
}