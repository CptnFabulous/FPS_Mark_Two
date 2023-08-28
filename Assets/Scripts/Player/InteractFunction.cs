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

    public Interactable lookingAt
    {
        get => current;
        set
        {
            if (value == current) return;
            current = value;
            window.Refresh(this);
        }
    }
    Interactable current;

    void OnInteract()
    {
        if (lookingAt != null && lookingAt.CanInteract(player))
        {
            lookingAt.OnInteract(player);
        }
    }

    void Update()
    {
        bool hit = Physics.Raycast(aimTransform.position, aimTransform.forward, out RaycastHit target, interactRange, interactionMask);
        lookingAt = hit ? target.collider.GetComponent<Interactable>() : null;
    }

    
}
