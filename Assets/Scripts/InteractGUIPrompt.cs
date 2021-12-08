using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractGUIPrompt : MonoBehaviour
{
    [Header("UI elements")]
    public Text name;
    public Text action;
    public GUIButtonPrompt prompt;
    public Image statusIcon;
    public Image progressBar;

    Interactable current;

    public void Refresh(InteractFunction function)
    {
        current = function.LookingAt;
        gameObject.SetActive(current != null);
        if (current == null)
        {
            return;
        }
        name.text = current.name;
        prompt.Refresh(function.input);
    }
    void Awake()
    {
        gameObject.SetActive(false);
    }
    private void LateUpdate()
    {
        if (current == null)
        {
            return;
        }

        if (current.Progress > 0 && current.Progress < 1)
        {
            action.text = current.inProgressMessage;
        }
        else if (current.active == false)
        {
            action.text = current.disabledMessage;
        }
        else
        {
            action.text = current.promptMessage;
        }

        progressBar.fillAmount = current.Progress;
    }
}
