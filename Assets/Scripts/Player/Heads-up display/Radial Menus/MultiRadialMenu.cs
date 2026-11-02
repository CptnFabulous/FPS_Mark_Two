using CptnFabulous.MiscUtility;
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
        TrySetNewMenu(GetCurrentBestMenu(), false, false);
        currentMenu = null;
    }
    private void Start()
    {
        if (openMenuInput != null) openMenuInput.onActionPerformed.AddListener(ProcessGenericMenuInput);
        if (menuSwitchInput != null) menuSwitchInput.onActionPerformed.AddListener(CycleMenus);
        directionalInput.onActionPerformed.AddListener(InputDirection);
    }

    #region Inputs
    public void ProcessGenericMenuInput(InputAction.CallbackContext context) => ProcessMenuInput(GetCurrentBestMenu(), context, false);
    public void ProcessSingleMenuInput(int index, InputAction.CallbackContext context) => ProcessMenuInput(menus[index], context, true);
    public void ProcessMenuInput(RadialMenu menu, InputAction.CallbackContext context, bool asSingleMenu)
    {
        // If input was a tap, quickly swap to the previous value
        if (menu != null && context.duration < InputSystem.settings.defaultTapTime)
        {
            menu.SwapToPreviousValue();
            return;
        }

        TrySetNewMenu(menu, context.ReadValueAsButton(), asSingleMenu);
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
            index = MathUtility.LoopIndex(index + 1, menus.Count);
            // End loop once one is found
            if (menus[index].optionsPresent) break;
        }

        // If the only valid menu is the current one, do nothing
        if (index == oldIndex) return;

        SwitchToDifferentMenu(menus[index]);
    }
    #endregion

    /// <summary>
    /// Attempts to switch to a new menu. If successful, alter GUI states to match based on the new menu and the state
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="active">Do we want the window to be open or not? Automatically disables if conditions are not met.</param>
    /// <param name="asSingleMenu">If true, will not be able to switch to a new menu until the current one closes.</param>
    /// <returns>If false, means another menu is active and blocked the new one.</returns>
    bool TrySetNewMenu(RadialMenu menu, bool active, bool asSingleMenu)
    {
        #region Attempt to assign new states

        // Do nothing if the specified menu is not part of this system (unless it's null, 
        if (menu != null && !menus.Contains(menu)) return false;
        // Don't allow this menu to be overridden, if another one is already open
        if (menuIsOpen && currentMenu != menu) return false;
        // Assign menu
        currentMenu = menu;

        // If the call was to open the menu, but said menu shouldn't be opened, ensure it's shut.
        if (currentMenu == null || !currentMenu.optionsPresent) active = false;
        // Set new active state
        menuIsOpen = active;

        #endregion

        #region Update GUI and inputs to match new menu and state

        // Enable/disable window
        canvasGroup.alpha = menuIsOpen ? 1 : 0;
        canvasGroup.interactable = menuIsOpen;
        canvasGroup.blocksRaycasts = menuIsOpen;

        // Disable actions we don't want the player performing while the menu is open
        foreach (SingleInput input in inputsToDisable) input.enabled = !menuIsOpen;
        // Activate inputs for directions and switching between menus
        directionalInput.enabled = menuIsOpen;
        menuSwitchInput.enabled = menuIsOpen;

        // Set up stuff for if it's one single menu or not
        switchPrompt.SetActive(!asSingleMenu);
        onOneSingleMenu = asSingleMenu;

        if (currentMenu != null)
        {
            // Open or close the menu
            if (menuIsOpen)
            {
                SwitchToDifferentMenu(currentMenu);
            }
            else
            {
                currentMenu.SetMenuActive(false);
            }
        }
        
        #endregion

        return true;
    }

    /// <summary>
    /// If the current menu is null or can't be accessed, check for a menu with options present.
    /// </summary>
    /// <returns></returns>
    RadialMenu GetCurrentBestMenu()
    {
        RadialMenu menu = currentMenu;
        if (menu == null || !menu.optionsPresent)
        {
            menu = menus.Find((m) => m.optionsPresent);
        }
        return menu;
    }

    void SwitchToDifferentMenu(RadialMenu newMenu)
    {
        currentMenu.SetMenuActive(false, false);
        currentMenu = newMenu;
        currentMenu.SetMenuActive(true);

        // Trigger animation
        int index = menus.IndexOf(currentMenu);
        animator.SetInteger(menuIndexName, index);
    }

}