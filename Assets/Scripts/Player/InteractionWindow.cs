using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class InteractionWindow : MonoBehaviour
{
    public InteractionHandler following;
    public Camera guiCamera;

    [Header("UI elements")]
    public CanvasGroup canvasGroup;
    public RectTransform highlightParent;
    public RectTransform highlight;
    public TMP_Text interactableName;
    public TMP_Text action;
    public GUIButtonPrompt prompt;
    public Image progressBar;

    [Header("Physics interaction")]
    public string pickupText = "Pick up";
    public string deadText = "(dead)";

    public PropCarryingHandler objectCarrier => following.objectCarrier;
    
    private void LateUpdate()
    {
        bool somethingToInteractWith = following.targetedInteractable != null;
        bool somethingToPickUp = objectCarrier != null && following.targetedPhysicsProp != null;
        bool somethingPresent = somethingToInteractWith || somethingToPickUp;
        canvasGroup.alpha = somethingPresent ? 1 : 0;
        if (!somethingPresent) return;

        /*
        // TO DO: have a bit here to gain the renderer/bounds and position the interaction window accordingly
        if (highlight != null)
        {
            Camera cam = following.referenceCamera;
            Rect hr = BoundsToRect(following.targetBounds, cam);

            //DebugDrawScreenLine(hr.min, hr.max, cam, Color.yellow);


            //highlight.anchoredPosition = hr.position;
            //highlight.sizeDelta = hr.size;

            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(highlightParent, hr.position, guiCamera, out Vector2 pos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(highlightParent, hr.size, guiCamera, out Vector2 size);


            highlight.anchoredPosition = pos;
            highlight.sizeDelta = size;
            //highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            //highlight.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            

            //highlight.rect.Set(hr.x, hr.y, hr.width, hr.height);
        }
        */

        if (somethingToInteractWith)
        {
            DisplayInteractable(following.targetedInteractable);
        }
        else if (somethingToPickUp)
        {
            DisplayPhysicsProp(following.targetedPhysicsProp);
        }
    }
    void DisplayInteractable(Interactable target)
    {
        bool canInteract = target.CanInteract(following.player);
        SetInteractability(canInteract);

        string displayName = target.displayName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            Entity e = target.parentEntity;
            displayName = (e != null) ? e.name : target.name;
        }
        interactableName.text = displayName;

        // Set values
        float progress = target.Progress;
        progressBar.fillAmount = progress;
        if (progress > 0 && progress < 1)
        {
            action.text = target.inProgressMessage;
        }
        else
        {
            action.text = canInteract ? target.promptMessage : target.disabledMessage;
        }
    }
    void DisplayPhysicsProp(Rigidbody target)
    {
        // Interactability should always be true - if it wasn't interactable, it shouldn't have been assigned in the first place
        SetInteractability(true);
        // Determine display name (add suffix for additional info e.g. if a character is dead)
        string name = target.name;

        Entity e = EntityCache<Entity>.GetEntity(target.gameObject);
        if (e != null)
        {
            name = e.properName;

            Health h = e.health;
            if (h != null && h.IsAlive == false) name += $" {deadText}";
        }

        interactableName.text = name;

        progressBar.fillAmount = 0;
        action.text = pickupText;
    }


    void SetInteractability(bool canInteract) => prompt.inputDisabled = !canInteract; // Set active states of button prompt and status icon

    /*
    private void OnDrawGizmos()
    {
        if (following.referenceCamera == null) return;
        if (Camera.current != following.referenceCamera) return;

        bool somethingToInteractWith = following.targetedInteractable != null;
        bool somethingToPickUp = objectCarrier != null && following.targetedPhysicsProp != null;
        bool somethingPresent = somethingToInteractWith || somethingToPickUp;
        if (!somethingPresent) return;

        Bounds b = following.targetBounds;
        Gizmos.DrawWireCube(b.center, b.size);
        //Gizmos.DrawGUITexture(BoundsToRect(b, following.referenceCamera), progressBar.mainTexture);
    }
    */
    public static void DebugDrawScreenLine(Vector2 a, Vector2 b, Camera camera, Color colour, float duration = 0)
    {
        Vector3 a3 = a;
        Vector3 b3 = b;
        a3.z = 1;
        b3.z = 1;
        Debug.DrawRay(camera.ScreenToWorldPoint(a3), camera.ScreenToWorldPoint(b3), colour, duration);
    }

    static readonly Vector3[] cubeCorners = new Vector3[]
    {
        new Vector3(-1, -1, -1),
        new Vector3(1, -1, -1),
        new Vector3(-1, -1, 1),
        new Vector3(1, -1, 1),

        new Vector3(-1, 1, -1),
        new Vector3(1, 1, -1),
        new Vector3(-1, 1, 1),
        new Vector3(1, 1, 1),
    };

    public static Rect BoundsToRect(Bounds bounds, Camera cam)
    {
        Vector3 extents = bounds.extents;
        // Input the first value. If it starts at zero but no part of the bounds is inside zero, then the size will be inaccurate.
        Vector2 startPos = cam.WorldToScreenPoint(bounds.center);
        Rect final = new Rect(startPos, Vector2.zero);
        for (int i = 0; i < 8; i++)
        {
            // Create corner in world space
            Vector3 corner = cubeCorners[i];
            corner.x *= extents.x;
            corner.y *= extents.y;
            corner.z *= extents.z;
            corner += bounds.center;

            // Calculate screen position and add it to the final rect
            Vector2 screenPoint = cam.WorldToScreenPoint(corner);
            final.min = Vector2.Min(final.min, screenPoint);
            final.max = Vector2.Max(final.max, screenPoint);
        }

        final.position = startPos;
        return final;
    }
}