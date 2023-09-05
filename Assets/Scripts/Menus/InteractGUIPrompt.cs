using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractGUIPrompt : MonoBehaviour
{
    [Header("UI elements")]
    public Text interactableName;
    public Text action;
    public GUIButtonPrompt prompt;
    public Image statusIcon;
    public Image progressBar;

    InteractFunction following;
    Interactable current => following.lookingAt;
    Player player => following.player;
    bool canInteract => following.canInteract;

    public void Refresh(InteractFunction function)
    {
        following = function;

        gameObject.SetActive(current != null);

        if (current == null) return;

        // Set name
        interactableName.text = current.name;
        // Assign button prompt
        UnityEngine.InputSystem.PlayerInput input = player.controls;
        prompt.AssignAction(input.actions.FindAction(function.interactInputName), input);
    }
    private void LateUpdate()
    {
        gameObject.SetActive(current != null);
        if (gameObject.activeSelf == false) return;

        prompt.gameObject.SetActive(canInteract);
        statusIcon.gameObject.SetActive(!canInteract);

        progressBar.fillAmount = current.Progress;
        if (current.Progress > 0 && current.Progress < 1)
        {
            action.text = current.inProgressMessage;
        }
        else
        {
            action.text = canInteract ? current.promptMessage : current.disabledMessage;
        }
    }
}
