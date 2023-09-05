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

    public Interactable lookingAt { get; private set; }
    public bool canInteract { get; private set; }

    void OnInteract()
    {
        if (lookingAt == null) return;

        if (lookingAt.CanInteract(player))
        {
            lookingAt.OnInteract(player);
        }
    }

    void Update()
    {
        bool hit = Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit target, interactRange, interactionMask);
        // Determine if the player is looking at an interactable
        Interactable nowLookingAt = hit ? target.collider.GetComponent<Interactable>() : null;
        // Determine if the player can interact with what they're looking at
        canInteract = (nowLookingAt != null) ? nowLookingAt.CanInteract(player) : false;

        // If the player is looking at something different to before, update the interaction window
        if (nowLookingAt != lookingAt)
        {
            lookingAt = nowLookingAt;
            window.Refresh(this);
        }
    }
}
