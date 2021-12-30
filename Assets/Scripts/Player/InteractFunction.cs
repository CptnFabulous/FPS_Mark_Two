using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractFunction : MonoBehaviour
{
    public Player player;
    public Transform aimTransform;
    public InteractGUIPrompt window;
    
    [Header("Stats")]
    public float interactRange = 3;
    public LayerMask interactionMask = ~0;
    public readonly string interactInputName = "Interact";

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

    void OnInteract()
    {
        if (LookingAt != null && LookingAt.active)
        {
            LookingAt.onInteract.Invoke(player);
        }
    }

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
    }

    
}
