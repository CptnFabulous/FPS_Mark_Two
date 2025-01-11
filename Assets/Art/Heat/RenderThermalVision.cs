using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// WIP: The second, hopefully more optimised version of my thermal vision code.
/// </summary>
public class RenderThermalVision : ScriptableRendererFeature
{
    public Material thermalVisionMaterial;
    public LayerMask smokeLayers = LayerMask.GetMask("Smoke");
    public float smokeAlpha = 0.1f;

    [Header("Render data")]
    public LayerMask renderLayers = ~0;
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

    class ThermalPass : ScriptableRenderPass
    {
        Material thermalVisionMaterial;
        Material backgroundMaterial;
        LayerMask smokeLayers;
        float smokeAlpha = 0.1f;
        FilteringSettings opaqueForOpaques;
        FilteringSettings opaqueForTransparents;

        string m_ProfilerTag = "Thermal Render Pass";

        List<ShaderTagId> shaderTags = new List<ShaderTagId>()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward"),
        };


        public ThermalPass(Material viewMaterial, Material backgroundMaterial, LayerMask renderLayers, LayerMask smokeLayers, float smokeAlpha)
        {
            // Get a mask of everything that needs to have an effect over it, minus what needs to be rendered separately as transparent.
            LayerMask opaqueMask = renderLayers & ~smokeLayers;
            opaqueForOpaques = new FilteringSettings(RenderQueueRange.opaque, opaqueMask);
            opaqueForTransparents = new FilteringSettings(RenderQueueRange.transparent, opaqueMask);

            // Set the more cosmetic values
            this.thermalVisionMaterial = viewMaterial;

            this.smokeLayers = smokeLayers;
            this.smokeAlpha = smokeAlpha;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            using (new ProfilingSample(cmd, m_ProfilerTag))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                CameraData cameraData = renderingData.cameraData;
                Camera camera = cameraData.camera;
                //if (cameraData.isStereoEnabled) context.StartMultiEye(camera);

                // Reset depth data so everything renders right over the original scene
                cameraData.clearDepth = true;


                // TO DO: Draw skybox/colour background
                SortingCriteria sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;

                // Figure out ambient heat value, and draw as a base over everything that doesn't need unique data shown
                DrawingSettings ambientPassSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortFlags);
                ambientPassSettings.overrideMaterial = GetMaterial(ObjectHeat.ambientTemperature, 1);
                context.DrawRenderers(renderingData.cullResults, ref ambientPassSettings, ref opaqueForOpaques);
                context.DrawRenderers(renderingData.cullResults, ref ambientPassSettings, ref opaqueForTransparents);

                // Then render specific objects that have unique heat data
                foreach (ObjectHeat heat in ObjectHeat.activeHeatSources)
                {
                    foreach (Renderer r in heat.renderers)
                    {
                        // Don't try rendering if it's been destroyed for whatever reason
                        if (r == null) continue;

                        // Don't write data for objects that aren't being viewed by the camera
                        int rendererLayer = r.gameObject.layer;
                        if (MiscFunctions.IsLayerInLayerMask(camera.cullingMask, rendererLayer) == false) continue;

                        // Determines if renderer should be see-through
                        bool isSmoke = MiscFunctions.IsLayerInLayerMask(smokeLayers, rendererLayer);
                        float alpha = isSmoke ? smokeAlpha : 1f;

                        // Generates material based off values, and applies it to each sub-mesh of the renderer
                        Material m = GetMaterial(heat.degreesCelsius, alpha);
                        for (int i = 0; i < r.materials.Length; i++)
                        {
                            //cmd.DrawRenderer(r, m, i);
                            // Make sure it only draws one render pass.
                            // For some reason if I don't specify that it'll draw a heap of extra passes that mess up the desired look.
                            cmd.DrawRenderer(r, m, i, 0);
                        }
                    }
                }
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        




        static Dictionary<(float, float), Material> materialCache = new Dictionary<(float, float), Material>();

        Material GetMaterial(float temperature, float alpha)
        {
            (float, float) key = (temperature, alpha);
            if (materialCache.TryGetValue(key, out Material m)) return m;

            // Instantiate a new material
            Material newMaterial = Instantiate(thermalVisionMaterial);

            // Assign values
            float tempRatio = Mathf.InverseLerp(ObjectHeat.minHeat, ObjectHeat.maxHeat, temperature);
            newMaterial.SetFloat("Temperature", tempRatio);
            newMaterial.SetFloat("Opacity", alpha);

            materialCache[key] = newMaterial;
            return newMaterial;
        }
    }

    ThermalPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        // Create a new pass instance
        m_ScriptablePass = new ThermalPass(thermalVisionMaterial, backgroundMaterial, renderLayers, smokeLayers, smokeAlpha);

        // Specify render pass event
        m_ScriptablePass.renderPassEvent = renderPassEvent;
        /*
        // Get a mask of everything that needs to have an effect over it, minus what needs to be rendered separately as transparent.
        LayerMask opaqueMask = renderLayers & ~smokeLayers;
        m_ScriptablePass.opaqueForOpaques = new FilteringSettings(RenderQueueRange.opaque, opaqueMask);
        m_ScriptablePass.opaqueForTransparents = new FilteringSettings(RenderQueueRange.transparent, opaqueMask);
        m_ScriptablePass.thermalVisionMaterial = thermalVisionMaterial;
        m_ScriptablePass.smokeLayers = smokeLayers;
        m_ScriptablePass.smokeAlpha = smokeAlpha;
        */
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


