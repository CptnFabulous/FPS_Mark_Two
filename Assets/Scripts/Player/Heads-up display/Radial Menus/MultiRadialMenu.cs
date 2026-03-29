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
    public GameObject switchPrompt;

    RadialMenu currentMenu;
    public bool menuIsOpen { get; private set; } = false;
    bool onOneSingleMenu = false;

    private void Awake()
    {
        SetMenuSystemActive(false);
        currentMenu = null;
    }
    private void Start()
    {
        if (openMenuInput != null) openMenuInput.onActionPerformed.AddListener((ctx) => SetMenuSystemActive(ctx.ReadValueAsButton()));
        if (menuSwitchInput != null) menuSwitchInput.onActionPerformed.AddListener(CycleMenus);
        directionalInput.onActionPerformed.AddListener(InputDirection);
    }
    
    /// <summary>
    /// Activates the whole menu system to the last-selected menu, and allows switching between different menus in a single activation
    /// </summary>
    /// <param name="active"></param>
    public void SetMenuSystemActive(bool active)
    {
        switchPrompt.SetActive(true);

        // If the current menu is null or can't be accessed, check for a menu with options present
        RadialMenu menu = currentMenu;
        if (menu == null || !menu.optionsPresent)
        {
            menu = menus.Find((m) => m.optionsPresent);
        }

        // Attempt to switch. If successful, alter functionality and GUI visibility to match
        if (SetMenuActiveState(menu, active))
        {
            switchPrompt.SetActive(true);
            onOneSingleMenu = false;
        }
    }
    /// <summary>
    /// Opens one specific menu and doesn't allow it to be switched from until it shuts
    /// </summary>
    /// <param name="index"></param>
    /// <param name="active"></param>
    public void SetSingleMenuActive(int index, bool active) => SetSingleMenuActive(menus[index], active);
    public void SetSingleMenuActive(RadialMenu menu, bool active)
    {
        // Attempt to switch. If successful, alter functionality and GUI visibility to match
        if (SetMenuActiveState(menu, active))
        {
            switchPrompt.SetActive(false);
            onOneSingleMenu = true;
        }
    }
    public void ProcessSingleMenuInput(int index, InputAction.CallbackContext context)
    {
        // If input was a tap, quickly swap to the previous value
        if (context.duration < InputSystem.settings.defaultTapTime)
        {
            // TO DO: swap to last value
            menus[index].SwapToPreviousValue();
            return;
        }
        SetSingleMenuActive(index, context.ReadValueAsButton());
    }

    bool SetMenuActiveState(RadialMenu menu, bool active)
    {
        // Do nothing if the specified menu is not part of this system
        if (!menus.Contains(menu)) return false;

        // If a different menu is already open, do nothing with this input and wait for it to close.
        if (menuIsOpen && currentMenu != menu) return false;

        // Assign menu
        currentMenu = menu;

        // if the desired menu shouldn't be opened, ensure it's shut.
        if (currentMenu == null || !currentMenu.optionsPresent) active = false;

        menuIsOpen = active;

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
        if (menuIsOpen)
        {
            SwitchToDifferentMenu(currentMenu);
            //currentMenu.EnterMenu();
        }
        else
        {
            currentMenu.ExitMenu();
        }

        return true;
    }
    void InputDirection(InputAction.CallbackContext context)
    {
        if (currentMenu == null) return;

        Vector2 direction = context.ReadValue<Vector2>();
        currentMenu.InputDirection(direction, !directionalInput.usingGamepad);
    }
    void CycleMenus(InputAction.CallbackContext context)
    {
        // TO DO: don't do anything if system is currently exclusively opening a single menu
        if (onOneSingleMenu) return;

        if (currentMenu == null) return;

        if (context.ReadValueAsButton() == false) return;

        // Get current menu index
        int oldIndex = menus.IndexOf(currentMenu);

        // Cycle through menus until one is found with options present
        int index = oldIndex;
        for (int i = 0; i < menus.Count; i++)
        {
            index = MiscFunctions.LoopIndex(index + 1, menus.Count);
            // End loop once one is found
            if (menus[index].optionsPresent) break;
        }

        // If the only valid menu is the current one, do nothing
        if (index == oldIndex) return;

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