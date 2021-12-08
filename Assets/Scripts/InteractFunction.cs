using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractFunction : MonoBehaviour
{
    public Player player;
    public Transform aimTransform;

    public CustomInput.Button interact = new CustomInput.Button(KeyCode.E, CustomInput.ControllerButton.North);
    
    [Header("Stats")]
    public float interactRange = 3;
    public LayerMask interactionMask = ~0;

    [Header("UI elements")]
    public GameObject interactWindow;
    public Text name;
    public Text action;
    public GUIButtonPrompt prompt;

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

            interactWindow.SetActive(current != null);
            if (current != null)
            {
                name.text = current.name;
                action.text = current.prompt;
                prompt.Refresh(interact);
            }
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

        if (LookingAt != null && interact.Pressed)
        {
            LookingAt.onInteract.Invoke(player);
        }
    }
}
