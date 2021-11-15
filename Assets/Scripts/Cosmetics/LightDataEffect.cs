using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDataEffect : LerpingCosmeticEffect
{
    public Light lightSource;
    public float minIntensity = 0;
    public float maxIntensity = 1;
    public float minRange = 0;
    public float maxRange = 10;
    public Gradient colourGradient;
    public override void SetLerpDirectly(float value)
    {
        lightSource.color = colourGradient.Evaluate(value);
        lightSource.range = Mathf.Lerp(minRange, maxRange, value);
        lightSource.intensity = Mathf.Lerp(minIntensity, maxIntensity, value);
    }
}
