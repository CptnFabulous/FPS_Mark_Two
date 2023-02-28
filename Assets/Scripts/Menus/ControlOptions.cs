using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ControlOptions : OptionsMenu
{
    public PlayerInput controls;
    public InputActionAsset defaultControls;

    [Header("Setup")]
    public RectTransform bindingWindow;
    public Text instructions;

    [Header("Binding prefabs")]
    public Text mapNamePrefab;
    public Text actionNamePrefab;
    public BindingOption bindingPrefab;
    public Text compositeNamePrefab;
    public BindingOption compositeBindingPrefab;

    public List<BindingOption> allBindingOptions { get; private set; }
    BindingOption pending;
    float windowHeight;
    InputAction getPressedKeyForUpdatingBinding;

    public override void ApplySettings()
    {
        
    }
    public override void ObtainCurrentValues()
    {
        for (int i = 0; i < allBindingOptions.Count; i++)
        {
            allBindingOptions[i].Refresh();
        }
    }
    #region Setting up menu
    public override void SetupOptions()
    {
        controls = GetComponentInParent<PlayerInput>();
        if (controls != null)
        {
            SetupAsset(controls.actions);
        }
        else
        {
            SetupAsset(defaultControls);
        }
        

        getPressedKeyForUpdatingBinding = new InputAction("Get New Key");
        getPressedKeyForUpdatingBinding.AddBinding("/*/<button>");
        getPressedKeyForUpdatingBinding.performed += GetCurrentButtonPressed;
    }
    void SetupAsset(InputActionAsset asset)
    {
        mapNamePrefab.gameObject.SetActive(false);
        actionNamePrefab.gameObject.SetActive(false);
        bindingPrefab.gameObject.SetActive(false);
        compositeNamePrefab.gameObject.SetActive(false);
        compositeBindingPrefab.gameObject.SetActive(false);
        allBindingOptions = new List<BindingOption>();
        for (int m = 0; m < asset.actionMaps.Count; m++)
        {
            SetupMap(asset.actionMaps[m]);
        }
        bindingWindow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowHeight);
    }
    void SetupMap(InputActionMap map)
    {
        // Instantiate title
        Text mapNameText = Instantiate(mapNamePrefab, bindingWindow);
        mapNameText.gameObject.name = "Map: " + map.name;
        mapNameText.text = map.name;
        ArrangeRectInColumn(mapNameText.rectTransform);

        // For each map, setup each appropriate action
        for (int a = 0; a < map.actions.Count; a++)
        {
            SetupAction(map.actions[a]);
        }
    }
    void SetupAction(InputAction action)
    {
        Text actionNameText = Instantiate(actionNamePrefab, bindingWindow);
        actionNameText.gameObject.name = "Action: " + action.name;
        actionNameText.text = action.name;
        ArrangeRectInColumn(actionNameText.rectTransform);

        // A composite binding is made by several consecutive InputBinding structs for an action. One with 'isComposite' plus several more after it.
        // According to this link, anyway https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/ActionBindings.html

        for (int b = 0; b < action.bindings.Count; b++)
        {
            // Set up options for each binding. Since bindings are structs, reference them by index so it links back to the correct action
            InputBinding binding = action.bindings[b];
            if (binding.isComposite)
            {
                // Create title for composite binding, but no path
                Text compositeNameText = Instantiate(compositeNamePrefab, bindingWindow);
                compositeNameText.gameObject.name = "Composite: " + binding.name;
                compositeNameText.text = binding.name;
                ArrangeRectInColumn(compositeNameText.rectTransform);
            }
            else if (binding.isPartOfComposite) // If binding is part of the current composite
            {
                // Create an option showing a name and path
                BindingOption compositeBinding = Instantiate(compositeBindingPrefab, bindingWindow);
                compositeBinding.gameObject.name = "Composite binding: " + binding.name;
                compositeBinding.SetupBinding(action, b, this);
                ArrangeRectInColumn(compositeBinding.rectTransform);
                allBindingOptions.Add(compositeBinding);
            }
            else // New binding is not a composite
            {
                BindingOption bindingOption = Instantiate(bindingPrefab, bindingWindow);
                bindingOption.gameObject.name = "Binding: " + action.name + " #" + b + 1;
                bindingOption.SetupBinding(action, b, this);
                ArrangeRectInColumn(bindingOption.rectTransform);
                allBindingOptions.Add(bindingOption);
            }

            // If a binding represents a composite, it will have a name and the 'path' will represent the composite type
            // If a binding is part of a composite, it will have a name and a path
            // If a binding is not part of a composite, it will have a path but no name
        }
    }
    void ArrangeRectInColumn(RectTransform rt)
    {
        rt.gameObject.SetActive(true);
        //rt.SetParent(bindingWindow);
        rt.anchoredPosition = new Vector3(0, -windowHeight, 0);
        windowHeight += rt.rect.height;
    }
    #endregion

    public void SetPending(BindingOption newPendingBinding)
    {
        if (allBindingOptions.Contains(newPendingBinding) == false) // If the binding parameter isn't part of this menu
        {
            return;
        }

        if (pending != null) // If another binding was already pending
        {
            //Debug.Log(pending + " is already pending, cancelling on frame " + Time.frameCount);
            return;
        }

        // Assign this as the current pending option in baseMenu
        pending = newPendingBinding;

        //controls.SwitchCurrentActionMap()
        // Enable new input detection in baseMenu
        pending.buttonToRebind.interactable = false;
        getPressedKeyForUpdatingBinding.Enable();
        instructions.text = "Press a button to assign it as the new binding.";
    }
    public void GetCurrentButtonPressed(InputAction.CallbackContext context)
    {
        string path = context.control.path;
        // If a binding is not assigned
        // OR due to a quirk with the input system, if the path registers as 'anyKey' rather than as an actual specific key
        if (pending == null || path.Contains("anyKey"))
        {
            return;
        }




        //if (context.control.)

        //if (context.control.device != controls.de)


        // Check the new path against the current binding path
        pending.CheckToAssignNewBinding(context.control.path, out BindingOption.BindingAssignResult result);
        switch (result)
        {
            case BindingOption.BindingAssignResult.Succeeded:
                // Binding change succeeded
                OnOptionsChanged();
                DisablePending();
                break;
            case BindingOption.BindingAssignResult.BindingIsSame:
                // Cancel and display a message that the prompt is the same
                instructions.text = pending + " already has this path assigned!";
                DisablePending();
                break;
            case BindingOption.BindingAssignResult.WrongControlType:
                // Display a message saying the control scheme is incompatible, but stay in pending mode
                instructions.text = pending + " does not accept that binding type!";
                break;
            case BindingOption.BindingAssignResult.AlreadyTaken:
                // Display a message saying the binding is already taken by another action, and show an option to swap the binding paths around
                instructions.text = pending + " cannot use this path, as it is already assigned to another binding!";
                return;
            default:
                // Cancel and display a generic 'binding attempt failed' message
                instructions.text = "Binding attempt failed (generic error).";
                DisablePending();
                break;
        }
    }
    void DisablePending()
    {
        getPressedKeyForUpdatingBinding.Disable();
        pending.buttonToRebind.interactable = true;
        pending = null;
    }
}
