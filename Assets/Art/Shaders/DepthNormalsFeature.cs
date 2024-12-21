using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthNormalsFeature : ScriptableRendererFeature
{
    DepthNormalsPass depthNormalsPass;

    public override void Create()
    {
        //Debug.Log("Creating depth/normals pass");
        depthNormalsPass = new DepthNormalsPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //Debug.Log("Adding depth/normals pass");
        RenderTargetHandle depthNormalsTexture = new RenderTargetHandle();
        depthNormalsTexture.Init("_CameraDepthNormalsTexture");
        depthNormalsPass.depthAttachmentHandle = depthNormalsTexture;

        RenderTextureDescriptor baseDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        baseDescriptor.colorFormat = RenderTextureFormat.ARGB32;
        baseDescriptor.depthBufferBits = depthNormalsPass.kDepthBufferBits;
        depthNormalsPass.descriptor = baseDescriptor;

        renderer.EnqueuePass(depthNormalsPass);
    }


    class DepthNormalsPass : ScriptableRenderPass
    {
        public int kDepthBufferBits => 32;

        // Formerly private and get/private set. Change these back later
        public RenderTargetHandle depthAttachmentHandle { get; set; }
        public RenderTextureDescriptor descriptor { get; set; }





        private Material depthNormalsMaterial = null;
        private FilteringSettings m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
        string m_ProfilerTag = "DepthNormals Prepass";
        ShaderTagId m_ShaderTagId = new ShaderTagId("DepthOnly");




        public DepthNormalsPass()
        {
            depthNormalsMaterial = CoreUtils.CreateEngineMaterial("Hidden/Internal-DepthNormalsTexture");
            renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
        }


        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(depthAttachmentHandle.id, descriptor, FilterMode.Point);
            ConfigureTarget(depthAttachmentHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

            // I don't know what the 'using' section is here for.
            // Since the one parameter is never used, the code inside can be taken out and it seems to function exactly the same.
            // EDIT: It seems that the constructor itself does some code to register data in the profiler
            using (new ProfilingSample(cmd, m_ProfilerTag))
            {
                // How come it executes the command buffer twice?
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;


                //ref CameraData cameraData = ref renderingData.cameraData;
                CameraData cameraData = renderingData.cameraData;
                Camera camera = cameraData.camera;
                if (cameraData.isStereoEnabled) context.StartMultiEye(camera);


                drawSettings.overrideMaterial = depthNormalsMaterial;


                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);

                cmd.SetGlobalTexture("_CameraDepthNormalsTexture", depthAttachmentHandle.id);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (depthAttachmentHandle == RenderTargetHandle.CameraTarget) return;

            cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
            depthAttachmentHandle = RenderTargetHandle.CameraTarget;
        }
    }
}