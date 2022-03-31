using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class BindingOption : MonoBehaviour//, ISelectHandler, IPointerEnterHandler
{
    public Text bindingName;
    public GUIButtonPrompt shownBinding;
    public Button buttonToRebind;
    public RectTransform rectTransform;




    InputAction action;
    int bindingIndex;
    string currentPathInMenu;
    ControlOptions baseMenu;

    InputBinding binding
    {
        get
        {
            return action.bindings[bindingIndex];
        }
    }
    static readonly List<char> bindingPunctuationToAddSpaces = new List<char>
    {
        '&'
    };
    static readonly List<char> bindingPunctuationToRemove = new List<char>
    {
        ';'
    };

    public void SetupBinding(InputAction newAction, int newBindingIndex, ControlOptions menu)
    {
        action = newAction;
        bindingIndex = newBindingIndex;
        baseMenu = menu;

        string displayBindingGroups = MiscFunctions.FormatNameForPresentation(binding.groups, bindingPunctuationToAddSpaces, bindingPunctuationToRemove);

        if (binding.isPartOfComposite)
        {
            string displayName = MiscFunctions.FormatNameForPresentation(binding.name);
            bindingName.text = displayName + " (" + displayBindingGroups + ")";
        }
        else
        {
            bindingName.text = displayBindingGroups;
        }
        
        shownBinding.enabled = false;
        buttonToRebind.onClick.AddListener(()=> menu.SetPending(this));
    }
    public void Refresh()
    {
        currentPathInMenu = binding.effectivePath;
        shownBinding.Refresh(binding);
    }
    public void CheckToAssignNewBinding(string newPath, out BindingAssignResult result)
    {
        result = BindingAssignResult.Succeeded;
        Debug.Log("Rebinding " + binding.name + ": " + currentPathInMenu + ", " + newPath + " (Frame #" + Time.frameCount + ")");
        
        // Format specific path to a generic version that accepts any of a particular input device
        // e.g. /Keyboard/a to <Keyboard>/a
        newPath = FormatPathToGeneric(newPath, out string newType);
        if (currentPathInMenu == newPath)
        {
            result = BindingAssignResult.BindingIsSame;
            return;
        }

        string oldType = PathType(currentPathInMenu);
        if (oldType != newType)
        {
            Debug.Log(newPath + " is the wrong type compared to " + currentPathInMenu);
            result = BindingAssignResult.WrongControlType;
            return;
        }

        // Check if path is not already being used by another binding in the same map
        for (int i = 0; i < baseMenu.allBindingOptions.Count; i++)
        {
            BindingOption existing = baseMenu.allBindingOptions[i];
            if (existing == this)
            {
                continue;
            }

            bool sameMap = existing.action.actionMap == action.actionMap;
            bool samePath = existing.currentPathInMenu == newPath;
            if (samePath && sameMap)
            {
                Debug.Log(newPath + " is already taken by " + existing + "!");
                result = BindingAssignResult.AlreadyTaken;
                return;
            }
        }

        // If previous checks are passed, set newBindingPath to the path
        currentPathInMenu = newPath;
        shownBinding.Refresh(currentPathInMenu);
        return;
    }
    public enum BindingAssignResult
    {
        Succeeded,
        BindingIsSame,
        WrongControlType,
        AlreadyTaken
    }

    public static string FormatPathToGeneric(string path, out string pathType)
    {
        pathType = "Other";

        for (int i = 0; i < kbmVariants.Length; i++)
        {
            if (path.Contains(kbmVariants[i]))
            {
                pathType = "Keyboard&Mouse";
                // Remove section featuring kbmVariants[i], plus the slash at the start
                int start = path.IndexOf(kbmVariants[i]);
                path = path.Remove(start, kbmVariants[i].Length);
                path = path.Remove(0, start);
                // Replace it with a generic keyboard or mouse 
                path = path.Insert(0, "<" + kbmVariants[i] + ">");
            }
        }

        for (int i = 0; i < gamepadVariants.Length; i++)
        {
            if (path.Contains(gamepadVariants[i]))
            {
                pathType = "Gamepad";
                // Remove original reference to specific gamepad, plus the slash at the start
                int start = path.IndexOf(gamepadVariants[i]);
                path = path.Remove(start, gamepadVariants[i].Length);
                path = path.Remove(0, start);
                // Insert generic gamepad reference
                path = path.Insert(0, "<Gamepad>");
            }
        }
        return path;
    }
    public static string PathType(string inputPath)
    {
        for (int i = 0; i < kbmVariants.Length; i++)
        {
            if (inputPath.Contains(kbmVariants[i]))
            {
                return "Keyboard&Mouse";
            }
        }
        for (int i = 0; i < gamepadVariants.Length; i++)
        {
            if (inputPath.Contains(gamepadVariants[i]))
            {
                return "Gamepad";
            }
        }
        return "Other";
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



    public void Apply()
    {
        action.ApplyBindingOverride(bindingIndex, currentPathInMenu);
    }
}
