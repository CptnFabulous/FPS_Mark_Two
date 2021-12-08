using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractFunction : MonoBehaviour
{
    public Player player;
    public Transform aimTransform;
    public InteractGUIPrompt window;
    public CustomInput.Button input = new CustomInput.Button(KeyCode.E, CustomInput.ControllerButton.North);
    
    [Header("Stats")]
    public float interactRange = 3;
    public LayerMask interactionMask = ~0;

    

    public Interactable LookingAt
    {
        get
        {
            return current;
        }
        set
        {
            if (value == current)
            {
                return;
            }

            current = value;

            window.Refresh(this);
        }
    }
    Interactable current;

    void Update()
    {
        if (Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit target, interactRange, interactionMask))
        {
            LookingAt = target.collider.GetComponent<Interactable>();
        }
        else
        {
            LookingAt = null;
        }

        if (input.Pressed && LookingAt != null && LookingAt.active)
        {
            LookingAt.onInteract.Invoke(player);
        }
    }
}
