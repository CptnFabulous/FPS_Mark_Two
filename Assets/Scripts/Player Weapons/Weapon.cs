using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    public Entity parentEntity;
    public Sprite hudGraphic;

    [Header("Attack modes")]
    public bool oneHanded;
    public bool isOffhand = false;
    public WeaponMode[] modes;
    public int currentModeIndex;

    [Header("Switching")]
    public float switchSpeed;
    public int indexOrder;
    public UnityEvent onDraw;
    public UnityEvent onHolster;
    public bool isSwitching { get; private set; }

    Character _user;

    public Character user
    {
        get
        {
            // Check if the weapon is still attached to the current user. Clear if not (e.g. if dropped)
            if (_user != null && transform.IsChildOf(_user.transform) == false) _user = null;
            // If now null, check for a new user

            return ComponentUtility.AutoCache(ref _user, gameObject, ComponentGetType.InParent);

            //return _user ??= GetComponentInParent<Character>();
        }
    }
    public WeaponMode CurrentMode
    {
        get
        {
            currentModeIndex = Mathf.Clamp(currentModeIndex, 0, modes.Length - 1);
            return modes[currentModeIndex];
        }
    }
    public bool InAction
    {
        get
        {
            if (isSwitching) return true;
            if (CurrentMode.inAttack) return true;
            if (CurrentMode.inSecondaryAction) return true;
            return false;
        }
    }
    public float switchTimeMultiplier => user.weaponHandler.switchTimeMultiplier * user.weaponHandler.switchTimeModifiers.calculatedValue;

    public IEnumerator Draw()
    {
        // Do necessary stuff to disable mode (but don't switch away from it)
        CurrentMode.enabled = false;

        isSwitching = true;

        gameObject.SetActive(true);
        onDraw.Invoke();

        // Wait to switch
        yield return new WaitForSeconds(switchSpeed * switchTimeMultiplier);

        // Enable current mode so it does all the necessary stuff it needs to do when first activated
        CurrentMode.enabled = true;

        isSwitching = false;
    }
    public IEnumerator Holster()
    {
        isSwitching = true;

        // Disable current mode without switching away from it
        yield return CurrentMode.EndAsync();

        onHolster.Invoke();
        yield return new WaitForSeconds(switchSpeed * switchTimeMultiplier);

        gameObject.SetActive(false);
        isSwitching = false;
    }
    public IEnumerator SwitchMode(int newModeIndex)
    {
        // Ignore if same mode
        if (newModeIndex == currentModeIndex) yield break;

        isSwitching = true;

        // End current mode
        yield return CurrentMode.EndAsync();

        // Officially switch modes
        currentModeIndex = newModeIndex;
        CurrentMode.onSwitch.Invoke();

        // Switch to new mode
        yield return CurrentMode.SwitchTo();
        CurrentMode.enabled = true;

        isSwitching = false;
    }
}