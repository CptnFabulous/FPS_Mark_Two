using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderBasedResourceMeter : ResourceDisplay
{
    [Header("Renderer")]
    [SerializeField] Graphic graphic;
    [SerializeField] Material material;
    [SerializeField] string fill = "_Fill_Amount";
    [SerializeField] string criticalThreshold = "_Fill_Critical_Threshold";
    [SerializeField] string normalColourName = "_Background_Colour";
    [SerializeField] string criticalColourName = "_Critical_Colour";
    [SerializeField] string referenceDimensionsName = "_Dimensions";

    Material cachedMaterial => graphic.material;
    public override Color safeColour
    {
        get => cachedMaterial.GetColor(normalColourName);
        set => cachedMaterial.SetColor(normalColourName, value);
    }
    public override Color criticalColour
    {
        get => cachedMaterial.GetColor(criticalColourName);
        set => cachedMaterial.SetColor(criticalColourName, value);
    }

    private void Awake()
    {
        graphic.material = new Material(material);
        cachedMaterial.SetVector(referenceDimensionsName, rectTransform.sizeDelta);
    }

    protected override void Refresh(Resource values)
    {
        cachedMaterial.SetFloat(fill, values.current / values.max);
        cachedMaterial.SetFloat(criticalThreshold, values.criticalLevel / values.max);
        base.Refresh(values);
    }
}