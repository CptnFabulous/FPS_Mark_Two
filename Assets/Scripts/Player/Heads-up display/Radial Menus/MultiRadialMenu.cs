using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiRadialMenu : MonoBehaviour
{
    public List<RadialMenu> menus;

    [Header("Inputs")]
    public SingleInput openMenuInput;
    public SingleInput menuSwitchInput;
    public SingleInput directionalInput;
    public SingleInput[] inputsToDisable;

    [Header("Visuals")]
    public Animator animator;
    public string menuIndexName = "Active Menu";
    public CanvasGroup canvasGroup;

    RadialMenu currentMenu;
    public bool menuIsOpen { get; private set; } = false;

    private void Awake()
    {
        SetMenuActive(false);
        currentMenu = null;
    }
    private void Start()
    {
        openMenuInput.onActionPerformed.AddListener(SetMenuActive);
        menuSwitchInput.onActionPerformed.AddListener(CycleMenus);
        directionalInput.onActionPerformed.AddListener(InputDirection);
    }

    void SetMenuActive(InputAction.CallbackContext context) => SetMenuActive(context.ReadValueAsButton());
    void SetMenuActive(bool active)
    {
        menuIsOpen = active;

        // If the current menu is null or can't be accessed, check for a menu with options present
        if (currentMenu == null || !currentMenu.optionsPresent)
        {
            currentMenu = menus.Find((m) => m.optionsPresent);
        }

        // If no valid menu is found, automatically hide
        if (currentMenu == null) menuIsOpen = false;

        // Enable/disable window
        canvasGroup.alpha = menuIsOpen ? 1 : 0;
        canvasGroup.interactable = menuIsOpen;
        canvasGroup.blocksRaycasts = menuIsOpen;

        // Disable actions we don't want the player performing while the menu is open
        foreach (SingleInput input in inputsToDisable) input.enabled = !menuIsOpen;
        // Activate inputs for directions and switching between menus
        directionalInput.enabled = menuIsOpen;
        menuSwitchInput.enabled = menuIsOpen;

        // Open or close the menu
        if (currentMenu == null) return;
        if (menuIsOpen)
        {
            SwitchToDifferentMenu(currentMenu);
            //currentMenu.EnterMenu();
        }
        else
        {
            currentMenu.ExitMenu();
        }
    }
    void InputDirection(InputAction.CallbackContext context)
    {
        if (currentMenu == null) return;

        Vector2 direction = context.ReadValue<Vector2>();
        currentMenu.InputDirection(direction, !directionalInput.usingGamepad);
    }
    void CycleMenus(InputAction.CallbackContext context)
    {
        if (currentMenu == null) return;

        if (context.ReadValueAsButton() == false) return;

        // Switch between the different active menus
        int index = menus.IndexOf(currentMenu);
        index = MiscFunctions.LoopIndex(index + 1, menus.Count);



        SwitchToDifferentMenu(menus[index]);
    }

    void SwitchToDifferentMenu(RadialMenu newMenu)
    {
        currentMenu.ExitMenu(false);
        currentMenu = newMenu;
        currentMenu.EnterMenu();

        // Trigger animation
        int index = menus.IndexOf(currentMenu);
        animator.SetInteger(menuIndexName, index);
    }
}
