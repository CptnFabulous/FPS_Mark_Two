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
    public Text mapTitlePrefab;
    public ControlBindingRow bindingRowPrefab;

    List<ControlBindingRow> allBindingRows = new List<ControlBindingRow>();
    int currentPendingBinding = -1;
    public bool BindingIsPendingChange
    {
        get
        {
            return MiscFunctions.WithinArray(currentPendingBinding, allBindingRows.Count);
        }
    }

    InputAction getPressedKeyForUpdatingBinding;


    public override void ApplySettings()
    {
        //throw new System.NotImplementedException();
        for (int i = 0; i < allBindingRows.Count; i++)
        {
            // Assign new option to current binding
        }
    }
    public override void ObtainCurrentValues()
    {
        for (int i = 0; i < allBindingRows.Count; i++)
        {
            allBindingRows[i].Refresh();
        }
        getPressedKeyForUpdatingBinding.Enable();
    }
    private void OnDisable()
    {
        getPressedKeyForUpdatingBinding.Disable();
    }
    public override void SetupOptions()
    {
        controls = GetComponentInParent<PlayerInput>();

        mapTitlePrefab.gameObject.SetActive(false);
        bindingRowPrefab.gameObject.SetActive(false);
        float windowHeight = 0;

        // Populate window with options for each binding
        for (int m = 0; m < controls.actions.actionMaps.Count; m++)
        {
            InputActionMap map = controls.actions.actionMaps[m];

            // Create title for the action map, to sort the inputs
            Text mapTitleObject = Instantiate(mapTitlePrefab, bindingWindow);
            mapTitleObject.gameObject.SetActive(true);
            mapTitleObject.gameObject.name = "Map Title - " + map.name;
            mapTitleObject.text = map.name;
            mapTitleObject.rectTransform.anchoredPosition = new Vector3(0, -windowHeight, 0);
            windowHeight += mapTitleObject.rectTransform.rect.height;

            // Look through each action to find bindings
            for (int a = 0; a < map.actions.Count; a++)
            {
                InputAction action = map.actions[a];


                // Instantiate binding row and determine its position
                ControlBindingRow bindingRow = Instantiate(bindingRowPrefab, bindingWindow);
                bindingRow.gameObject.SetActive(true);
                bindingRow.rectTransform.anchoredPosition = new Vector3(0, -windowHeight, 0);
                windowHeight += bindingRow.rectTransform.rect.height;

                // Populate binding row with appropriate action and name, and add listener to enable apply/revert buttons when changed.
                bindingRow.gameObject.name = "Binding Row - " + action.name;
                bindingRow.Setup(action, controls);
                bindingRow.onBindingChanged.AddListener(OnOptionsChanged);
                allBindingRows.Add(bindingRow);
            }
        }

        bindingWindow.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, windowHeight);





        getPressedKeyForUpdatingBinding = new InputAction(binding: "/*/<button>");
        getPressedKeyForUpdatingBinding.performed += GetCurrentButtonPressed;
    }









    void GetCurrentButtonPressed(InputAction.CallbackContext context)
    {
        if (!BindingIsPendingChange)
        {
            return;
        }
        ControlBindingRow binding = allBindingRows[currentPendingBinding];
        if (binding.assignedAction.type == InputActionType.Button)
        {
            binding.CheckToAssignNewBinding(context);
        }










        /*
        if (context.action.type != InputActionType.Button)
        {
            return;
        }
        */
    }
}