using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    bool buttonHeld;
    int frameChanged = int.MinValue;

    WeaponMode _current;
    Coroutine currentAction;

    public WeaponMode currentAbility
    {
        get => _current;
        set
        {
            if (_current == value) return;

            CancelCurrentAction();

            _current = value;

            //a.runtimeAnimatorController

            frameChanged = Time.frameCount;
        }
    }


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
        interactionHandler.input.onActionPerformed.AddListener((_) => CancelCurrentAction());
        //selectorMenuInput.onActionPerformed.AddListener((_) => menu.);


        base.Awake();
    }





    void OnAttack(InputAction.CallbackContext context)
    {
        buttonHeld = context.ReadValueAsButton();

        if (currentAbility == null) return;
        if (buttonHeld == false) return;
        if (currentAction != null) return;

        if (currentAbility.CanAttack() == false) return;
        currentAction = StartCoroutine(PerformOffhandAbility(currentAbility));
    }

    IEnumerator PerformOffhandAbility(WeaponMode offhandAbility)
    {
        // Put away current weapon (if two-handed)
        Weapon currentWeapon = weaponHandler.CurrentWeapon;
        if (currentWeapon != null && currentWeapon.oneHanded == false) yield return weaponHandler.SetCurrentWeaponDrawn(false);

        // TO DO: check if the offhand ability is standalone or attached to a weapon
        Weapon w = offhandAbility.attachedTo;
        if (w != null)
        {
            // Draw the weapon then switch to its mode
            w.gameObject.SetActive(true);
            yield return w.Draw();
            yield return w.SwitchMode(CollectionUtility.IndexOfInArray(w.modes, offhandAbility));

            // Set input, and wait until input and action are finished
            offhandAbility.SetPrimaryInput(true);
            yield return new WaitUntil(() => !buttonHeld || !offhandAbility.inAttack);
            // Let go of current input, and wait for action to end
            offhandAbility.SetPrimaryInput(false);
            yield return new WaitUntil(() => !offhandAbility.inAttack);

            // Put away weapon
            yield return w.Holster();
            w.gameObject.SetActive(false);
        }
        else
        {
            // Deploy offhand weapon
            offhandAbility.enabled = true;
            yield return offhandAbility.SwitchTo();

            // Perform offhand action
            Debug.Log("Starting attack");
            offhandAbility.SetPrimaryInput(true);
            // Wait until player finishes attack
            yield return new WaitUntil(() => !buttonHeld || !offhandAbility.inAttack);
            Debug.Log("Ending attack");
            offhandAbility.SetPrimaryInput(false); // Reset input for next time
            yield return new WaitUntil(() => !offhandAbility.inAttack);

            // Put away offhand weapon
            yield return offhandAbility.SwitchFrom();
            offhandAbility.enabled = false;
        }

        // Ensure current weapon is deployed
        currentAction = null;
        weaponHandler.SetCurrentWeaponActive(true);
    }

    void CancelCurrentAction(bool isSwitchingWeapons = false)
    {
        if (Time.frameCount <= frameChanged) return;

        if (currentAction != null)
        {
            StopCoroutine(currentAction);
            currentAction = null;
        }

        if (_current != null) _current.enabled = false;

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

    public override void SwitchMode(int index)
    {
        if (allModes.Count <= 0) return;

        currentAbility = allModes[index];
    }
}