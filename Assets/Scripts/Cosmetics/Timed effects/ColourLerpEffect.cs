using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColourLerpEffect : LerpingCosmeticEffect
{
    public Gradient colourGradient;
    public UnityEvent<Color> onColourLerp;

    public override void SetLerpDirectly(float value)
    {
        Color colour = colourGradient.Evaluate(value);
        onColourLerp.Invoke(colour);
    }
}
