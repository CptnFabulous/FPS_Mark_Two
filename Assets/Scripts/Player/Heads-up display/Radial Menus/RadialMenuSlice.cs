using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a 'slice' for the radial menu, whose size is set up to match a single segment by controlling an image's fill amount and rotation.
/// </summary>
public class RadialMenuSlice : MonoBehaviour
{
    public enum IconRotation
    {
        RelativeToSlice,
        MaintainUpwardAxis,
        Upright,
    }

    public RectTransform rectTransform;
    public RadialMenuSliceSizer visualHandler;
    public bool autoMatchParentMenuSegmentSize = false;
    public Image icon;
    public IconRotation iconRotation;
    public ResourceDisplay resourceDisplay;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField, Range(0, 360)] float _segmentSize;
#endif

    RadialMenu _parentMenu;

    public RadialMenu parentMenu => _parentMenu ??= GetComponentInParent<RadialMenu>(true);
    //public float segmentSize => _segmentSize;
    public Sprite sprite
    {
        get => icon.sprite;
        set => icon.sprite = value;
    }

#if UNITY_EDITOR
    private void OnValidate() => UpdateSegmentSize(_segmentSize);
#endif
    void Start()
    {
        if (autoMatchParentMenuSegmentSize) parentMenu.onValueChanged.AddListener((_) => UpdateSegmentSize(360 / parentMenu.numberOfOptions));
    }

    public void UpdateSegmentSize(float segmentSize)
    {
#if UNITY_EDITOR
        _segmentSize = segmentSize;
#endif
        // Run appropriate code to resize slice
        float rotation = -transform.localEulerAngles.z;
        if (visualHandler != null) visualHandler.Refresh(rotation, segmentSize);
        
        // Update icon rotation
        if (icon == null) return;
        RectTransform iconTransform = icon.rectTransform;
        iconTransform.localRotation = Quaternion.identity;
        switch (iconRotation)
        {
            case IconRotation.Upright:
                iconTransform.localRotation *= Quaternion.Inverse(transform.localRotation);
                break;

            case IconRotation.MaintainUpwardAxis:

                if (Vector3.Dot(iconTransform.up, parentMenu.transform.up) < 0)
                {
                    iconTransform.localRotation *= Quaternion.Euler(180, 0, 0);
                }

                break;
        }
    }
}