using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class BindingOption : MonoBehaviour//, ISelectHandler, IPointerEnterHandler
{
    public Text name;
    public GUIButtonPrompt shownBinding;
    public Button buttonToRebind;
    public RectTransform rectTransform;




    InputAction action;
    int bindingIndex;
    string newBindingPath;

    public void SetupBinding(InputAction newAction, int newBindingIndex)
    {
        action = newAction;
        bindingIndex = newBindingIndex;
        name.text = action.bindings[newBindingIndex].name;
        shownBinding.enabled = false;
    }
    public void SetupBinding(InputAction newAction, int newBindingIndex, string shownName)
    {
        action = newAction;
        bindingIndex = newBindingIndex;
        name.text = shownName;
        shownBinding.enabled = false;
    }

    public void Refresh()
    {
        InputBinding binding = action.bindings[bindingIndex];
        shownBinding.Refresh(binding);
    }

    













    /// <summary>
    /// Names of device types for keyboard and mouse control.
    /// </summary>
    public static readonly string[] kbmVariants = new string[]
    {
        "Keyboard",
        "Mouse",
    };
    /// <summary>
    /// Names of device types for gamepad control.
    /// </summary>
    public static readonly string[] gamepadVariants = new string[]
    {
        "Gamepad",
        "XInputController",
        "DualShockGamepad",
        "SwitchProControllerHID",
        "WebGLGamepad",
        "iOSGameController",
        "AndroidGamepad",
    };






    public void CheckToAssignNewBinding(InputAction.CallbackContext context)
    {
        /*
        if (binding.groups.Contains("Keyboard&Mouse") && binding.isComposite == false && binding.isPartOfComposite == false)
        {
                
        }
        if (binding.groups.Contains("Gamepad") && binding.isComposite == false && binding.isPartOfComposite == false)
        {
                
        }
        */

        for (int i = 0; i < action.actionMap.bindings.Count; i++)
        {
            // Check existing bindings for conflicts
        }
        

        string newPath = context.control.path;

        for (int i = 0; i < kbmVariants.Length; i++)
        {
            if (newPath.Contains(kbmVariants[i]))
            {
                //newKeyboardPath = newPath;
                break;
            }
        }

        for (int i = 0; i < gamepadVariants.Length; i++)
        {
            if (newPath.Contains(gamepadVariants[i]))
            {
                //newGamepadPath = newPath;
                break;
            }
        }
    }

    

    /*
    public void OnSelect(BaseEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
    */
}
