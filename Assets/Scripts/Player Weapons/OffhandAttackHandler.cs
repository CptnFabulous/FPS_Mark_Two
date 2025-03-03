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

    public WeaponMode currentAbility { get; set; }

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
        currentAction = StartCoroutine(PerformOffhandAbility(currentAbility));
    }


    public IEnumerator PerformOffhandAbility(WeaponMode offhandAbility)
    {
        // Put away current weapon (if two-handed)
        if (weaponHandler.CurrentWeapon.oneHanded == false) yield return weaponHandler.SetCurrentWeaponDrawn(false);

        // Deploy offhand weapon (if deploy animation is present)

        // Perform offhand action
        offhandAbility.SetPrimaryInput(true);
        // Wait until attack is complete
        yield return new WaitUntil(() => !buttonHeld || !offhandAbility.inAttack);
        offhandAbility.SetPrimaryInput(false); // Reset input for next time
        yield return new WaitUntil(() => !offhandAbility.inAttack);

        // Put away offhand weapon

        // Ensure current weapon is deployed
        currentAction = null;
        weaponHandler.SetCurrentWeaponActive(true);
    }
}
