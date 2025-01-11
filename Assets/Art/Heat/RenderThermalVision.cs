using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
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
    public RenderQueueType renderQueueType = RenderQueueType.Opaque;

    class ThermalPass : ScriptableRenderPass
    {
        public Material thermalVisionMaterial;
        public LayerMask smokeLayers;
        public float smokeAlpha = 0.1f;
        public FilteringSettings filteringSettings;

        string m_ProfilerTag = "Thermal Render Pass";

        List<ShaderTagId> shaderTags = new List<ShaderTagId>()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward"),
        };

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            //ProfilingSample sample = new ProfilingSample(cmd, m_ProfilerTag);
            
            using (new ProfilingSample(cmd, m_ProfilerTag))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                CameraData cameraData = renderingData.cameraData;
                Camera camera = cameraData.camera;
                //if (cameraData.isStereoEnabled) context.StartMultiEye(camera);

                // Reset depth data so everything renders right over the original scene
                cameraData.clearDepth = true;

                SortingCriteria sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortFlags);

                // Figure out ambient heat value
                drawSettings.overrideMaterial = GetMaterial(ObjectHeat.ambientHeat, 1);

                // TO DO: Draw skybox/colour background

                // Draw base colour based on ambient temperature
                // Use these links for more info:
                // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Rendering.ScriptableRenderContext.DrawRenderers.html
                // https://discussions.unity.com/t/rendering-objects-on-scene-using-only-scriptablerendercontext-drawrenderers/1519679/2 (info about disabling normal rendering while still showing additional render passes (involves changing culling mask before normal camera rendering, then changing back before the extra pass)
                //FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);

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
        m_ScriptablePass = new ThermalPass();

        m_ScriptablePass.renderPassEvent = renderPassEvent;
        RenderQueueRange range = renderQueueType switch
        {
            RenderQueueType.Transparent => RenderQueueRange.transparent,
            _ => RenderQueueRange.opaque
        };
        m_ScriptablePass.filteringSettings = new FilteringSettings(range, renderLayers);

        m_ScriptablePass.thermalVisionMaterial = thermalVisionMaterial;
        m_ScriptablePass.smokeLayers = smokeLayers;
        m_ScriptablePass.smokeAlpha = smokeAlpha;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


