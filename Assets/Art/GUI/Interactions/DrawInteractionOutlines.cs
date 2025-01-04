using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DrawInteractionOutlines : ScriptableRendererFeature
{
    public Material interactableHighlight;
    public Material nonInteractableHighlight;
    public Material physicsPropHighlight;
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

    class CustomRenderPass : ScriptableRenderPass
    {
        public Material interactableHighlight;
        public Material nonInteractableHighlight;
        public Material physicsPropHighlight;
        public InteractionHandler interactionHandler;
        
        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Check if this camera is on a player with an active interaction handler
            if (interactionHandler == null) return;
            if (interactionHandler.enabled == false) return;

            // Check if something has been hit
            RaycastHit rh = interactionHandler.hitData;
            if (rh.collider == null) return;

            Material m;
            Renderer[] childRenderers;

            // Check what the player is actually looking at.
            // Determine which highlight to use based on if it's a context-sensitive interaction or a physics object
            Interactable interactable = interactionHandler.targetedInteractable;
            Rigidbody physicsProp = interactionHandler.targetedPhysicsProp;
            if (interactable != null)
            {
                bool canInteract = interactionHandler.canInteractWithTarget;
                m = canInteract ? interactableHighlight : nonInteractableHighlight;
                childRenderers = ImmediateChildrenCache<Interactable, Renderer>.GetValues(interactable);
            }
            else if (physicsProp != null)
            {
                m = physicsPropHighlight;
                childRenderers = ImmediateChildrenCache<Rigidbody, Renderer>.GetValues(physicsProp);
            }
            else
            {
                // If no interaction is found, do nothing
                return;
            }

            // Setup command buffer
            CommandBuffer cmd = CommandBufferPool.Get("Interaction Outline Pass");
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // Draw all renderers that are part of the interactable
            LayerMask cameraMask = renderingData.cameraData.camera.cullingMask;
            foreach (Renderer r in childRenderers)
            {
                if (MiscFunctions.IsLayerInLayerMask(cameraMask, r.gameObject.layer) == false) return;
                //if (MiscFunctions.IsLayerInLayerMask(interactionHandler.detectionMask, r.gameObject.layer) == false) return;

                for (int i = 0; i < r.materials.Length; i++)
                {
                    // Make sure it only draws one render pass.
                    // For some reason if I don't specify that it'll draw a heap of extra passes that mess up the desired look.
                    //cmd.DrawRenderer(r, m, i);
                    cmd.DrawRenderer(r, m, i, 0);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();
        m_ScriptablePass.interactableHighlight = interactableHighlight;
        m_ScriptablePass.nonInteractableHighlight = nonInteractableHighlight;
        m_ScriptablePass.physicsPropHighlight = physicsPropHighlight;
        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = renderPassEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Get interaction handler
        Camera c = renderingData.cameraData.camera;
        Player p = c.GetComponentInParent<Player>();
        if (p == null) return;
        InteractionHandler ih = p.GetComponentInChildren<InteractionHandler>();
        if (ih == null) return;

        m_ScriptablePass.interactionHandler = ih;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


