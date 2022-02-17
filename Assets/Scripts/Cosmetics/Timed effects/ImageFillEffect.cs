using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFillEffect : LerpingCosmeticEffect
{
    public Image imageToChange;
    public override void SetLerpDirectly(float value)
    {
        imageToChange.fillAmount = value;
    }
}
