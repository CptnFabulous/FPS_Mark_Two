using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiAxisButtonPrompt : GUIButtonPromptBase
{
    [SerializeField] GUIButtonPrompt positive;
    [SerializeField] GUIButtonPrompt negative;
    [SerializeField] GUIButtonPrompt yPositive;
    [SerializeField] GUIButtonPrompt yNegative;

    private void Awake()
    {
        if (positive != null) positive.enabled = false;
        if (negative != null) negative.enabled = false;
        if (yPositive != null) yPositive.enabled = false;
        if (yNegative != null) yNegative.enabled = false;
    }

    protected override void DetermineCurrentBinding()
    {
        Debug.Log("Determining current binding");
        foreach (InputBinding binding in assignedInput.bindings)
        {
            Debug.Log(binding.groups);
            bool match = binding.groups.Contains(player.currentControlScheme);
            if (match == false) continue;
            if (binding.isPartOfComposite == false) continue;

            // Figure out correct prompts
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

            Debug.Log($"{binding.path}, {Time.frameCount}");
            prompt.Refresh(binding);
        }
    }
}