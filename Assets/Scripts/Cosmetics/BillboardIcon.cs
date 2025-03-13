using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BillboardIcon : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += UpdateIconRotation;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= UpdateIconRotation;
    }
    void UpdateIconRotation(ScriptableRenderContext context, Camera camera)
    {
        // Rotate the icon to face the currently rendering camera
        Transform ct = camera.transform;
        if (camera.orthographic)
        {
            // Match rotation directly if it's orthographic
            transform.rotation = ct.rotation;
        }
        else
        {
            // Otherwise rotate away from camera position
            transform.rotation = Quaternion.LookRotation(transform.position - ct.position, ct.up);
        }
    }
}