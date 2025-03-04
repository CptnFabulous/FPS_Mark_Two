using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OffhandAttackHandler : MonoBehaviour
{
    public WeaponMode[] abilities;


    public SingleInput input;
    public WeaponHandler weaponHandler;

    int abilityIndex = 0;

    bool buttonHeld;

    Coroutine currentAction;

    WeaponMode _current;

    public WeaponMode currentAbility
    {
        get => _current;
        set
        {
            if (_current == value) return;

            if (currentAction != null)
            {
                StopCoroutine(currentAction);
                currentAction = null;
            }

            if (_current != null) _current.enabled = false;

            _current = value;
        }
    }

    private void Awake()
    {
        currentAbility = abilities[abilityIndex];
        input.onActionPerformed.AddListener(OnAttack);
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        buttonHeld = context.ReadValueAsButton();
        if (buttonHeld == false) return;
        if (currentAction != null) return;

        if (currentAbility.CanAttack() == false) return;
        currentAction = StartCoroutine(PerformOffhandAbility(currentAbility));
    }


    public IEnumerator PerformOffhandAbility(WeaponMode offhandAbility)
    {
        Debug.Log("Performing offhand attack - " +  offhandAbility);

        // Put away current weapon (if two-handed)
        Weapon currentWeapon = weaponHandler.CurrentWeapon;
        if (currentWeapon != null && currentWeapon.oneHanded == false) yield return weaponHandler.SetCurrentWeaponDrawn(false);

        // Deploy offhand weapon
        offhandAbility.enabled = true;
        yield return offhandAbility.SwitchTo();

        // Perform offhand action
        Debug.Log("Starting attack");
        offhandAbility.SetPrimaryInput(true);
        // Wait until attack is complete
        yield return new WaitUntil(() => !buttonHeld || !offhandAbility.inAttack);
        Debug.Log("Ending attack");
        offhandAbility.SetPrimaryInput(false); // Reset input for next time
        yield return new WaitUntil(() => !offhandAbility.inAttack);

        // Put away offhand weapon
        yield return offhandAbility.SwitchFrom();
        offhandAbility.enabled = false;

        // Ensure current weapon is deployed
        currentAction = null;
        weaponHandler.SetCurrentWeaponActive(true);
    }
}
