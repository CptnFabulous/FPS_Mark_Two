using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class RadialMenuSliceSizer : MonoBehaviour
{
    public abstract void Refresh(float angle, float segmentSize);
}

public class ShaderBasedRadialSlice : RadialMenuSliceSizer
{
    [SerializeField] Graphic graphic;
    [SerializeField] Material material;
    [SerializeField] string segmentSize = "_Segment_Size";
    [SerializeField] string rotationAngle = "_Rotation_Angle";

    private void Awake()
    {
        graphic.material = new Material(material);
    }

    public override void Refresh(float angle, float size)
    {
        graphic.material.SetFloat(segmentSize, size);
        graphic.material.SetFloat(rotationAngle, angle);
    }
}