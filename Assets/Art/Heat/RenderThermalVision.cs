using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderThermalVision : ScriptableRendererFeature
{
    public Material thermalVisionMaterial;
    public Material backgroundMaterial;
    public LayerMask smokeLayers;
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
        FilteringSettings transparentForOpaques;
        FilteringSettings transparentForTransparents;

        string m_ProfilerTag = "Thermal Render Pass";
        LayerMask renderLayers;
        static Material dpm;
        static Dictionary<(float, float), Material> materialCache = new Dictionary<(float, float), Material>();
        //static List<Renderer> everythingToRender = new List<Renderer>();

        List<ShaderTagId> shaderTags = new List<ShaderTagId>()
        {
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward"),
        };

        /// <summary>
        /// An ultra simple opaque material, drawn to force an initial depth pass.
        /// </summary>
        static Material depthPassMaterial => dpm ??= CoreUtils.CreateEngineMaterial("Unlit/Color");

        public ThermalPass(Material thermalVisionMaterial, Material backgroundMaterial, LayerMask renderLayers, LayerMask smokeLayers, float smokeAlpha)
        {
            // Get a mask of everything that needs to have an effect over it, minus what needs to be rendered separately as transparent.
            this.renderLayers = renderLayers;
            LayerMask opaqueMask = renderLayers & ~smokeLayers;
            opaqueForOpaques = new FilteringSettings(RenderQueueRange.opaque, opaqueMask);
            opaqueForTransparents = new FilteringSettings(RenderQueueRange.transparent, opaqueMask);
            transparentForOpaques = new FilteringSettings(RenderQueueRange.opaque, smokeLayers);
            transparentForTransparents = new FilteringSettings(RenderQueueRange.transparent, smokeLayers);

            // Set the more cosmetic values
            this.thermalVisionMaterial = thermalVisionMaterial;
            if (backgroundMaterial != null)
            {
                this.backgroundMaterial = Instantiate(backgroundMaterial);
            }
            this.smokeLayers = smokeLayers;
            this.smokeAlpha = smokeAlpha;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            /*
            using (new ProfilingSample(cmd, m_ProfilerTag))
            {
                
            }
            */

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CameraData cameraData = renderingData.cameraData;
            Camera camera = cameraData.camera;
            if (cameraData.xr.enabled) context.StartMultiEye(camera);

            // Reset depth data so everything renders right over the original scene
            cameraData.clearDepth = true;

            // Draw background override material, but only if camera is set to render a background in the first place
            if (backgroundMaterial != null && camera.clearFlags != CameraClearFlags.Nothing)
            {
                // Assign values (we don't need to instantiate multiple materials because we're only rendering 1 temperature)
                float tempRatio = Mathf.InverseLerp(ObjectHeat.minHeat, ObjectHeat.maxHeat, ObjectHeat.ambientTemperature);
                backgroundMaterial.SetFloat("_Temperature", tempRatio);
                backgroundMaterial.SetFloat("_Opacity", 1);
                // Draw using command buffer and immediately execute, to ensure it goes behind everything else
                cmd.DrawProcedural(Matrix4x4.identity, backgroundMaterial, 0, MeshTopology.Triangles, 3);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }


            SortingCriteria sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;




            // TO DO:
            // Figure out how to use RenderStateBlocks to override depth passes, so I don't need to have a couple of RenderObjects passes beforehand to ensure depth is calculated
            // Ideally I'd like to render depth for everything, without actually having that stuff show on screen.

            /*
            RenderStateBlock stateBlock = new RenderStateBlock(RenderStateMask.Everything);
            //RenderStateBlock stateBlock = new RenderStateBlock();
            //stateBlock.mask |= RenderStateMask.Depth;
            //stateBlock.depthState = new DepthState();

            // Draw initial opaque pass, to force depth calculations for everything that needs to be opaque
            Debug.Log("Drawing initial opaque pass");
            DrawingSettings depthPassSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortFlags);
            depthPassSettings.overrideMaterial = depthPassMaterial;
            context.DrawRenderers(renderingData.cullResults, ref depthPassSettings, ref opaqueForOpaques, ref stateBlock);
            context.DrawRenderers(renderingData.cullResults, ref depthPassSettings, ref opaqueForTransparents, ref stateBlock);
            */



            int shaderRenderQueue = thermalVisionMaterial.shader.renderQueue;
            if (shaderRenderQueue <= RenderQueueRange.transparent.upperBound && shaderRenderQueue >= RenderQueueRange.transparent.lowerBound)
            //if (true)
            {
                // Draw initial opaque pass, to force depth calculations for everything that needs to be opaque
                Debug.Log("Drawing initial opaque pass");
                DrawingSettings depthPassSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortFlags);
                depthPassSettings.overrideMaterial = depthPassMaterial;
                context.DrawRenderers(renderingData.cullResults, ref depthPassSettings, ref opaqueForOpaques);
                context.DrawRenderers(renderingData.cullResults, ref depthPassSettings, ref opaqueForTransparents);
            }

            // Figure out ambient heat value, and draw as a base over everything that doesn't need unique data shown
            DrawingSettings ambientPassSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortFlags);
            ambientPassSettings.overrideMaterial = GetMaterial(ObjectHeat.ambientTemperature, 1);
            context.DrawRenderers(renderingData.cullResults, ref ambientPassSettings, ref opaqueForOpaques);
            context.DrawRenderers(renderingData.cullResults, ref ambientPassSettings, ref opaqueForTransparents);

            // Draw another similar pass, for transparent objects with no heat value assigned
            DrawingSettings ambientTransparentPassSettings = CreateDrawingSettings(shaderTags, ref renderingData, sortFlags);
            ambientTransparentPassSettings.overrideMaterial = GetMaterial(ObjectHeat.ambientTemperature, smokeAlpha);
            context.DrawRenderers(renderingData.cullResults, ref ambientTransparentPassSettings, ref transparentForOpaques);
            context.DrawRenderers(renderingData.cullResults, ref ambientTransparentPassSettings, ref transparentForTransparents);

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
                    if (MiscFunctions.IsLayerInLayerMask(renderLayers, rendererLayer) == false) continue;
                    //if (MiscFunctions.IsLayerInLayerMask(camera.cullingMask & renderLayers, rendererLayer) == false) continue;

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

            //context.ExecuteCommandBuffer(cmd);
            //cmd.Clear();

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
        Material GetMaterial(float temperature, float alpha)
        {
            (float, float) key = (temperature, alpha);
            if (materialCache.TryGetValue(key, out Material m)) return m;

            // Instantiate a new material
            Material newMaterial = Instantiate(thermalVisionMaterial);

            // Assign values
            float tempRatio = Mathf.InverseLerp(ObjectHeat.minHeat, ObjectHeat.maxHeat, temperature);
            newMaterial.SetFloat("_Temperature", tempRatio);
            newMaterial.SetFloat("_Opacity", alpha);

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
        m_ScriptablePass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


