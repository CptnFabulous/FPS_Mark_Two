using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskedFillRadialSlice : RadialMenuSliceSizer
{
    public Image background;
    public float degreePadding;
    public RectTransform contentParent;

    float totalPadding => degreePadding * 2;

    public override void Refresh(float angle, float segmentSize)
    {
        background.fillMethod = Image.FillMethod.Radial360;
        background.fillOrigin = 2;
        background.fillClockwise = true;

        float fillDegrees = segmentSize - totalPadding;
        background.fillAmount = fillDegrees / 360;

        Quaternion offsetRotation = Quaternion.Euler(0, 0, fillDegrees / 2);

        background.rectTransform.localRotation = offsetRotation;

        if (contentParent == null) return;
        Quaternion inverse = Quaternion.Inverse(offsetRotation);
        contentParent.localRotation = inverse;
    }
}