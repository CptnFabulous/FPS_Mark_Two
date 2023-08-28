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

    Interactable current;
    Player player;

    public void Refresh(InteractFunction function)
    {
        current = function.lookingAt;
        gameObject.SetActive(current != null);

        if (current == null) return;

        player = function.player;

        interactableName.text = current.name;

        UnityEngine.InputSystem.PlayerInput input = function.player.controls;
        prompt.AssignAction(input.actions.FindAction(function.interactInputName), input);
    }
    void Awake()
    {
        gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        if (current == null) return;

        bool interactable = current.CanInteract(player);

        if (current.Progress > 0 && current.Progress < 1)
        {
            action.text = current.inProgressMessage;
        }
        else if (interactable == false)
        {
            action.text = current.disabledMessage;
        }
        else
        {
            action.text = current.promptMessage;
        }

        prompt.gameObject.SetActive(interactable);
        statusIcon.gameObject.SetActive(!interactable);

        progressBar.fillAmount = current.Progress;
    }
}
