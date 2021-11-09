using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageColourEffect : LerpingCosmeticEffect
{
    public Image imageToChange;
    public Gradient colourGradient;

    public override void SetLerpDirectly(float value)
    {
        imageToChange.color = colourGradient.Evaluate(value);
    }
}
