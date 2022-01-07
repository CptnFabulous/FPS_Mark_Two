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
    public GUIButtonPrompt currentBindingPrompt;

    [Header("Binding prefabs")]
    public Text mapNamePrefab;
    public Text actionNamePrefab;
    public BindingOption bindingPrefab;
    public Text compositeNamePrefab;
    public BindingOption compositeBindingPrefab;

    List<BindingOption> allBindingOptions = new List<BindingOption>();
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
        getPressedKeyForUpdatingBinding.Enable();
    }
    public override void SetupOptions()
    {
        controls = GetComponentInParent<PlayerInput>();

        SetupAsset(controls.actions);

        getPressedKeyForUpdatingBinding = new InputAction(binding: "/*/<button>");
        getPressedKeyForUpdatingBinding.performed += GetCurrentButtonPressed;
    }


    private void OnDisable()
    {
        getPressedKeyForUpdatingBinding.Disable();
    }


    string allNames;
    void SetupAsset(InputActionAsset asset)
    {
        allNames = "All bindings";


        mapNamePrefab.gameObject.SetActive(false);
        actionNamePrefab.gameObject.SetActive(false);
        bindingPrefab.gameObject.SetActive(false);
        compositeNamePrefab.gameObject.SetActive(false);
        compositeBindingPrefab.gameObject.SetActive(false);
        for (int m = 0; m < asset.actionMaps.Count; m++)
        {
            SetupMap(asset.actionMaps[m]);
        }
        bindingWindow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowHeight);

        Debug.Log(allNames);
    }
    void SetupMap(InputActionMap map)
    {
        allNames += "\n" + map.name;

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
        allNames += "\n    " + action.name;

        Text actionNameText = Instantiate(actionNamePrefab, bindingWindow);
        actionNameText.gameObject.name = "Action: " + action.name;
        actionNameText.text = action.name;
        ArrangeRectInColumn(actionNameText.rectTransform);


        // A composite binding is made by several consecutive InputBinding structs for an action. One with 'isComposite' plus several more after it.
        // According to this link, anyway.
        // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/ActionBindings.html

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
                compositeBinding.SetupBinding(action, b);
                ArrangeRectInColumn(compositeBinding.rectTransform);
                allBindingOptions.Add(compositeBinding);
            }
            else // New binding is not a composite
            {
                BindingOption bindingOption = Instantiate(bindingPrefab, bindingWindow);
                string shownName = binding.groups;
                bindingOption.gameObject.name = "Binding: " + shownName;
                bindingOption.SetupBinding(action, b, shownName);
                ArrangeRectInColumn(bindingOption.rectTransform);
                allBindingOptions.Add(bindingOption);
            }

            // If a binding represents a composite, it will have a name and the 'path' will represent the composite type
            // If a binding is part of a composite, it will have a name and a path
            // If a binding is not part of a composite, it will have a path but no name
            allNames += "\n        " + binding.name + ": " + binding.path;
        }

    }




    void ArrangeRectInColumn(RectTransform rt)
    {
        rt.gameObject.SetActive(true);
        //rt.SetParent(bindingWindow);
        rt.anchoredPosition = new Vector3(0, -windowHeight, 0);
        windowHeight += rt.rect.height;
    }


    void GetCurrentButtonPressed(InputAction.CallbackContext context)
    {
        Debug.Log(context.action.activeControl.path);

        /*
        if (context.action.type != InputActionType.Button)
        {
            return;
        }
        */
    }
}
