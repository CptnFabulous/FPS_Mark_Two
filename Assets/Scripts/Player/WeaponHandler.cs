using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WeaponHandler : WeaponHandlerBase
{
    [Header("Inputs")]
    public SingleInput primaryInput;
    public SingleInput secondaryInput;
    public SingleInput tertiaryInput;
    public SingleInput weaponMenuInput;

    [Header("Additional weapon data")]
    public OffhandAttackHandler offhandAttacks;
    public ThrowHandler throwHandler;

    [Header("Stats")]
    public AmmunitionInventory ammo;
    public AimSwayHandler swayHandler;
    public WeaponSelectorHUD selectorInfo;
    public NumberKeySelector hotkeyHandler;

    [Header("Accessibility")]
    public ADSHandler adsHandler;
    public bool toggleADS;
    public bool quickSwitchModes = true;

    [Header("Events")]
    public UnityEvent<Weapon> onDraw;
    public UnityEvent<Weapon> onHolster;
    public UnityEvent<Weapon> onSwitchWeapon;

    public bool disableADS { get; set; }
    public bool isSwitching { get; private set; }

    public bool weaponDrawn
    {
        get => CurrentWeapon != null && CurrentWeapon.gameObject.activeSelf == true;
        set => SetCurrentWeaponActive(value);
    }
    /// <summary>
    /// Is the player currently in ADS on a particular weapon?
    /// </summary>
    public bool IsUsingADS(out RangedAttack r)
    {
        r = adsHandler.currentAttack;
        if (r == null) return false;

        return adsHandler.currentlyAiming;
    }
    public bool WeaponReady
    {
        get
        {
            if (!enabled) return false;
            // If player is not in the middle of switching weapons
            if (isSwitching) return false;
            // If player has a weapon equipped
            if (CurrentWeapon == null) return false;
            // If player is not in the middle of switching firing modes
            if (CurrentWeapon.isSwitching) return false;
            return true;
        }
    }
    public Transform aimAxis => swayHandler.aimAxis;
    public Vector3 AimDirection => swayHandler.aimDirection;
    public float aimSwayAngle => swayHandler.aimSwayAngle;


    protected override void Awake()
    {
        base.Awake();

        // Weapon and mode switch inputs
        hotkeyHandler.onSelectionMade.AddListener(OnWeaponHotkey);

        // Make it so the current weapon is automatically put away if the player dies
        controller.health.onDeath.AddListener((_) => weaponDrawn = false);

        // Primary and secondary fire inputs
        primaryInput.onActionPerformed.AddListener((ctx) => PrimaryFireInput(ctx.ReadValueAsButton()));
        secondaryInput.onActionPerformed.AddListener((ctx) => SecondaryFireInput(ctx.ReadValueAsButton()));
        tertiaryInput.onActionPerformed.AddListener((ctx) =>
        {
            if (ctx.ReadValueAsButton() == true) TertiaryInput();
        });
    }
    
    #region Inputs
    void PrimaryFireInput(bool pressed)
    {
        if (!WeaponReady) return;
        CurrentWeapon.CurrentMode.SetPrimaryInput(pressed);
    }
    void SecondaryFireInput(bool pressed)
    {
        if (!WeaponReady) return;
        CurrentWeapon.CurrentMode.SetSecondaryInput(pressed);
    }
    void TertiaryInput()
    {
        if (!WeaponReady) return;
        CurrentWeapon.CurrentMode.OnTertiaryInput();
    }
    void OnWeaponHotkey(int index)
    {
        if (menu.menuIsOpen) return;
        SwitchMode(index);
    }
    void OnScrollWeapon(InputValue input)
    {
        if (isSwitching) return; // Wait until any previous switch operation has finished
        if (menu.menuIsOpen) return; // Don't allow any other kinds of selection if the radial menu is open
        
        float inputValue = input.Get<float>();
        if (inputValue == 0) return; // If there's no input, do nothing

        int increment = Mathf.RoundToInt(Mathf.Sign(inputValue));

        // Switch either your weapon, or the firing mode on your current weapon
        if (quickSwitchModes && CurrentWeapon != null)
        {
            int newIndex = MathUtility.LoopIndex(CurrentWeapon.currentModeIndex + increment, CurrentWeapon.modes.Length);
            StartCoroutine(CurrentWeapon.SwitchMode(newIndex));
        }
        else if (allWeapons.Count > 0) // Don't allow switching if there's nothing to switch to
        {
            
            int newIndex = MathUtility.LoopIndex(equippedWeaponIndex + increment, allWeapons.Count);
            Weapon w = allWeapons[newIndex];
            StartCoroutine(SwitchWeapon(w));
        }
    }
    #endregion

    #region Weapon switching

    protected override void Refresh()
    {
        base.Refresh();

        selectorInfo.Refresh(this);

        if (CurrentWeapon != null) StartCoroutine(SwitchWeapon(CurrentWeapon));
    }

    public override void SwitchMode(WeaponMode mode) => StartCoroutine(SwitchWeaponAndFiringMode(mode));

    public void SetCurrentWeaponActive(bool drawn) => StartCoroutine(SetCurrentWeaponDrawn(drawn));
    public IEnumerator SetCurrentWeaponDrawn(bool drawn)
    {
        // If the desired state is already met, do nothing
        if (weaponDrawn == drawn) yield break;
        // If there's no weapon to draw/holster, do nothing
        if (CurrentWeapon == null) yield break;

        // If a weapon is active, holster it
        // If a current weapon is selected but not active, draw it
        UnityEvent<Weapon> toInvoke = drawn ? onDraw : onHolster;
        IEnumerator coroutine = drawn ? CurrentWeapon.Draw() : CurrentWeapon.Holster();
        toInvoke.Invoke(CurrentWeapon);
        yield return coroutine;
    }
    
    IEnumerator SwitchWeapon(Weapon newWeapon)
    {
        if (allWeapons.Count <= 0) yield break;
        if (isSwitching) yield break; // Don't attempt another switch if in the middle of another switch operation

        if (weaponDrawn)
        {
            // Do nothing if the desired weapon is already active, or the current weapon is in the middle of another task
            if (newWeapon == CurrentWeapon) yield break;
            if (CurrentWeapon.InAction) yield break;
        }

        isSwitching = true;
        onSwitchWeapon.Invoke(newWeapon);
        
        yield return SetCurrentWeaponDrawn(false); // Wait to holster current weapon
        lastSetMode = newWeapon.CurrentMode;
        //equippedWeaponIndex = newIndex; // Switch to new weapon index
        yield return SetCurrentWeaponDrawn(true); // Wait to draw new weapon

        isSwitching = false;
    }
    IEnumerator SwitchWeaponAndFiringMode(WeaponMode mode)
    {
        yield return SwitchWeapon(mode.attachedTo);
        if (CurrentWeapon == null) yield break;
        yield return CurrentWeapon.SwitchMode(MiscFunctions.IndexOfInCollection(CurrentWeapon.modes, mode));
    }

    #endregion
}