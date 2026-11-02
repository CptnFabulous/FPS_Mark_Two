using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OffhandAttackHandler : WeaponHandlerBase
{
    [Header("Inputs")]
    public SingleInput input;

    [Header("References")]
    public WeaponHandler weaponHandler;
    public InteractionHandler interactionHandler;
    public OffhandSelectorHUD selectorInfo;

    [Header("Stats")]
    [Range(float.Epsilon, 1)] public float weaponSwitchTimeMultiplier = 1;

    bool buttonHeld;
    bool cancelledExternally = false;
    int frameChanged = int.MinValue;

    Coroutine currentAction;

    bool currentlyUsingOffhandAbility => currentAction != null;

    protected override void Awake()
    {
        // Set up inputs
        input.onActionPerformed.AddListener(OnAttack);

        // Set up inputs to cancel other actions
        weaponHandler.onSwitchWeapon.AddListener((_) =>
        {
            // TO DO: check if the user is switching to a one-handed or two-handed weapon. If the former, don't cancel?
            CancelCurrentAction(true);
        });

        // If the user presses the interact button, that acts as the 'cancel' button.
        interactionHandler.input.onActionPerformed.AddListener((_) =>
        {
            if (currentlyUsingOffhandAbility) cancelledExternally = true;
        });
        //selectorMenuInput.onActionPerformed.AddListener((_) => menu.);


        base.Awake();
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        buttonHeld = context.ReadValueAsButton();

        if (currentMode == null) return;
        if (buttonHeld == false) return;
        if (currentAction != null) return;

        if (currentMode.CanAttack() == false) return;
        currentAction = StartCoroutine(PerformOffhandAbility(currentMode));
    }

    IEnumerator PerformOffhandAbility(WeaponMode offhandAbility)
    {
        cancelledExternally = false;

        // Put away current weapon (if two-handed)
        Weapon currentWeapon = weaponHandler.CurrentWeapon;
        bool putAwayWeaponForOperation = currentWeapon != null && currentWeapon.oneHanded == false;
        if (putAwayWeaponForOperation)
        {
            // Increase switch speed to make things faster
            weaponHandler.switchTimeModifiers["Performing offhand attack"] = weaponSwitchTimeMultiplier;
            yield return weaponHandler.SetCurrentWeaponDrawn(false);
        }

        // Check if the offhand ability is standalone or attached to a weapon
        Weapon w = offhandAbility.attachedTo;
        bool isPartOfWeapon = w != null;

        // Deploy offhand ability (change based on if part of a weapon or not)
        if (isPartOfWeapon)
        {
            w.gameObject.SetActive(true);
            yield return w.Draw();
            yield return w.SwitchMode(w.modes.IndexOf(offhandAbility));
        }
        else
        {
            offhandAbility.enabled = true;
            yield return offhandAbility.SwitchTo();
        }

        // Set input, and wait until input and action are finished
        offhandAbility.SetPrimaryInput(true);
        yield return new WaitUntil(() =>
        {
            // If attack has finished, stop waiting
            if (!offhandAbility.inAttack) return true;
            // If player is no longer pressing button, stop waiting
            if (!buttonHeld) return true;
            // If cancel function has been triggered, stop waiting
            if (cancelledExternally) return true;

            return false;
        });

        // If not cancelled externally, signal that attack input is released and let attack end
        if (!cancelledExternally) yield return offhandAbility.StopAttackAsync();

        // Put away offhand ability (change based on if part of a weapon or not)
        if (isPartOfWeapon)
        {
            yield return w.Holster();
            w.gameObject.SetActive(false);
        }
        else
        {
            yield return offhandAbility.EndAsync();
        }

        // If we put away our weapon for this, bring it out again now that the action is over.
        if (putAwayWeaponForOperation) yield return weaponHandler.SetCurrentWeaponDrawn(true);
        
        // Ensure current action is totally cancelled
        CancelCurrentAction();
    }

    void CancelCurrentAction(bool isSwitchingWeapons = false)
    {
        if (Time.frameCount <= frameChanged) return;

        if (currentlyUsingOffhandAbility)
        {
            StopCoroutine(currentAction);
            currentAction = null;
        }

        if (currentMode != null) currentMode.enabled = false;

        // Ensure weapon switch speed is reset (in case it was modified for switching from two-handed weapons)
        weaponHandler.switchTimeModifiers["Performing offhand attack"] = 1;

        // Auto-deploy the last weapon, but not if this action was triggered by switching to a new one
        if (!isSwitchingWeapons) weaponHandler.SetCurrentWeaponActive(true);
    }

    protected override void Refresh()
    {
        CancelCurrentAction();

        foreach (Weapon w in allWeapons)
        {
            if (w != null) w.gameObject.SetActive(false);
        }

        base.Refresh();

        selectorInfo.PopulateMenu(this);
    }
    public override void SwitchMode(WeaponMode value)
    {
        if (lastSetMode == value) return;

        CancelCurrentAction();

        lastSetMode = value;

        //a.runtimeAnimatorController

        frameChanged = Time.frameCount;
    }
}