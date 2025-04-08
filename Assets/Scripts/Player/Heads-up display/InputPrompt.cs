using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPrompt : MonoBehaviour
{
    public bool inputDisabled;
    [SerializeField] GUIButtonPrompt positive;
    [SerializeField] GUIButtonPrompt negative;
    [SerializeField] GUIButtonPrompt yPositive;
    [SerializeField] GUIButtonPrompt yNegative;

    InputAction assignedInput = null;
    PlayerInput player = null;

    public void AssignAction(InputAction newInput, PlayerInput newPlayer)
    {
        //if (player != null) player.onControlsChanged -= (_) => UpdateCurrentBinding();
        assignedInput = newInput;
        player = newPlayer;
        //if (player != null) player.onControlsChanged += (_) => UpdateCurrentBinding();
        //UpdateCurrentBinding();
    }

    //void OnDestroy() => AssignAction(null, null); // Clear values, including clearing listeners from player input
    private void LateUpdate() => UpdateCurrentBinding();

    void UpdateCurrentBinding()
    {
        if (assignedInput == null || player == null) return;
        // Disable object if values aren't present

        // I need to ensure that the existing values 
        positive.gameObject.SetActive(false);
        negative.gameObject.SetActive(false);
        yPositive.gameObject.SetActive(false);
        yNegative.gameObject.SetActive(false);


        // Cycle through bindings, seeing which ones are active
        foreach (InputBinding binding in assignedInput.bindings)
        {
            // Look through existing bindings, ignore ones that aren't relevant to current control scheme
            bool match = binding.groups.Contains(player.currentControlScheme);
            if (match == false) continue;

            // If binding is not part of a composite, see if it's a single binding
            if (binding.isPartOfComposite == false)
            {
                // If binding is neither composite nor part of composite, that means it's a single binding
                if (binding.isComposite == false)
                {
                    // Bind just one of the graphics and disable the others
                    positive.gameObject.SetActive(true);
                    positive.inputDisabled = inputDisabled;
                    positive.Refresh(binding);
                    negative.gameObject.SetActive(false);
                    yPositive.gameObject.SetActive(false);
                    yNegative.gameObject.SetActive(false);
                    break;
                }

                continue;
            }

            // Figure out correct prompts, ignore if no valid binding is present
            GUIButtonPrompt prompt = binding.name switch
            {
                "positive" => positive,
                "negative" => negative,
                "up" => yPositive,
                "down" => yNegative,
                "left" => negative,
                "right" => positive,
                _ => null
            };
            if (prompt == null) continue;

            prompt.gameObject.SetActive(true);
            prompt.inputDisabled = inputDisabled;
            prompt.Refresh(binding);
        }
    }
}