using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a 'slice' for the radial menu, whose size is set up to match a single segment by controlling an image's fill amount and rotation.
/// </summary>
public class RadialMenuFilledSlice : MonoBehaviour
{
    [SerializeField] RadialMenu menu;
    [SerializeField] float degreePadding;
    [SerializeField] Image highlight;
#if UNITY_EDITOR
    [SerializeField, Range(0, 360)] float testValue;
#endif

    public float segmentSize
    {
        get => (highlight.fillAmount * 360) + totalPadding;
        set
        {
            float fillDegrees = value - totalPadding;
            highlight.fillAmount = fillDegrees / 360;
            highlight.rectTransform.localRotation = Quaternion.Euler(0, 0, fillDegrees / 2);
#if UNITY_EDITOR
            testValue = value;
#endif
        }
    }
    float totalPadding => degreePadding * 2;

#if UNITY_EDITOR
    private void OnValidate() => segmentSize = testValue;
#endif
    void Awake()
    {
        highlight.fillMethod = Image.FillMethod.Radial360;
        highlight.fillOrigin = 2;
        highlight.fillClockwise = true;

        menu.onValueChanged.AddListener((_) => segmentSize = 360 / menu.numberOfOptions);
    }
}
