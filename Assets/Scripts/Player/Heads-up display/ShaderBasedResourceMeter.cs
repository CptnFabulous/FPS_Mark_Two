using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class ShaderBasedResourceMeter : ResourceDisplay
{
    [Header("Renderer")]
    [SerializeField] Graphic graphic;
    [SerializeField] Material material;
    [SerializeField] string fill = "_Fill_Amount";
    [SerializeField] string criticalThreshold = "_Fill_Critical_Threshold";

    private void Awake()
    {
        graphic.material = new Material(material);
    }

    protected override void Refresh(Resource values)
    {
        graphic.material.SetFloat(fill, values.current / values.max);
        graphic.material.SetFloat(criticalThreshold, values.criticalLevel / values.max);
        base.Refresh(values);
    }
}