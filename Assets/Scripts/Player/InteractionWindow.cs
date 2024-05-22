using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionWindow : MonoBehaviour
{
    public InteractionHandler following;

    [Header("UI elements")]
    public CanvasGroup canvasGroup;
    public TMP_Text interactableName;
    public TMP_Text action;
    public GUIButtonPrompt prompt;
    public Image progressBar;

    [Header("Physics interaction")]
    public string pickupText = "Pick up";
    public string deniedText = "Too heavy";

    public PropCarryingHandler objectCarrier => following.objectCarrier;

    private void Awake()
    {
        Debug.Log("Awaking");
        // Assign button prompt
        PlayerInput input = following.player.controls;
        InputAction action = following.input.action;
        prompt.AssignAction(action, input);
    }

    private void LateUpdate()
    {
        bool somethingToInteractWith = following.targetedInteractable != null;
        bool somethingToPickUp = objectCarrier != null && following.targetedPhysicsProp != null;
        bool somethingPresent = somethingToInteractWith || somethingToPickUp;
        canvasGroup.alpha = somethingPresent ? 1 : 0;
        if (!somethingPresent) return;
        

        // TO DO: have a bit here to gain the renderer/bounds and position the interaction window accordingly


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
        float progress = target.Progress;

        SetInteractability(canInteract);

        // Set values
        interactableName.text = target.name;
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
        // Set values
        SetInteractability(true);
        interactableName.text = target.name;
        progressBar.fillAmount = 0;
        action.text = "Pick up";
    }


    void SetInteractability(bool canInteract)
    {
        // Set active states of button prompt and status icon

        prompt.inputDisabled = !canInteract;
    }





    /*
    public static Rect BoundsToRect(Bounds bounds, Camera referenceCamera)
    {
        Vector2 startPos = referenceCamera.WorldToScreenPoint(bounds.min);
        Rect final = new Rect(startPos, Vector2.zero); // Input the first value. If it starts at zero but no part of the bounds is inside zero, then the size will be inaccurate.
        for (int x = -1; x < 2; x += 2)
        {
            for (int y = -1; y < 2; y += 2)
            {
                for (int z = -1; z < 2; z += 2)
                {
                    // Calculate each corner. Since x, y and z will always be either -1 or 1, multiplying the extents by every combination will result in every corner
                    Vector3 corner = bounds.extents;
                    corner.x *= x;
                    corner.y *= y;
                    corner.z *= z;
                    corner += bounds.center;
                    // Obtain the screen space, and expand the Rect to encompass it
                    Vector2 screenPoint = referenceCamera.WorldToScreenPoint(corner);
                    final.min = Vector2.Min(final.min, screenPoint);
                    final.max = Vector2.Max(final.max, screenPoint);
                }
            }
        }

        return final;
    }
    */
}