using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WeaponHandler : MonoBehaviour
{
    public Player controller;

    [Header("Weapons")]
    public Weapon[] equippedWeapons;
    public WeaponMode offhand;
    public Transform holdingSocket;
    public AmmunitionInventory ammo;
    public RadialMenu weaponSelector;
    public WeaponSelectorHUD selectorInfo;

    [Header("Stats")]
    public Transform aimAxis;
    public float standingAccuracy = 1;
    public float swaySpeed = 0.5f;
    public bool toggleADS;
    public bool quickSwitchModes = true;

    [Header("Events")]
    public UnityEvent<Weapon> onDraw;
    public UnityEvent<Weapon> onHolster;
    public UnityEvent<Weapon> onSwitchWeapon;

    int equippedWeaponIndex = 0;

    //public bool PrimaryHeld { get; private set; }
    public bool SecondaryActive { get; private set; }
    public bool isSwitching { get; private set; }

    public Weapon CurrentWeapon => (equippedWeapons.Length > 0) ? equippedWeapons[equippedWeaponIndex] : null;
    /// <summary>
    /// The direction the player is currently aiming in, accounting for accuracy sway.
    /// </summary>
    public Vector3 AimDirection => aimAxis.rotation * WeaponUtility.AimSway(aimSwayAngle, swaySpeed) * Vector3.forward;
    public float aimSwayAngle
    {
        get
        {
            float totalSway = standingAccuracy;
            if (CurrentWeapon != null)
            {
                if (CurrentWeapon.CurrentMode is RangedAttack ra)
                {
                    totalSway += ra.stats.sway;
                }
            }
            return totalSway;
        }
    }
    public bool weaponDrawn
    {
        get => CurrentWeapon != null && CurrentWeapon.gameObject.activeSelf == true;
        set => SetCurrentWeaponActive(value);
    }
    /// <summary>
    /// Is the player currently in ADS on a particular weapon?
    /// </summary>
    public bool IsUsingADS
    {
        get
        {
            if (CurrentWeapon == null) return false;
            RangedAttack r = CurrentWeapon.CurrentMode as RangedAttack;
            return r != null && r.optics != null && r.optics.IsAiming;
        }
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



    private void Awake()
    {
        if (ammo == null) ammo = GetComponent<AmmunitionInventory>();
        
        weaponSelector.onValueConfirmed.AddListener(SwitchWeaponAndModeFromIndex);
    }
    private void Start()
    {
        UpdateAvailableWeapons();
    }
    private void Update()
    {
        if (weaponSelector.menuIsOpen == false && NumberKeySelector.NumKeyPressed(out int index, true))
        {
            SwitchWeaponAndModeFromIndex(index);
        }
    }
    

    #region Inputs
    void OnFire(InputValue input)
    {
        if (!WeaponReady) return;
        //PrimaryHeld = input.isPressed;
        CurrentWeapon.CurrentMode.SetPrimaryInput(input.isPressed);
    }
    void OnADS(InputValue input)
    {
        if (!WeaponReady) return;

        SecondaryActive = MiscFunctions.GetToggleableInput(SecondaryActive, input.isPressed, toggleADS);
        CurrentWeapon.CurrentMode.SetSecondaryInput(SecondaryActive);
    }
    void OnReload()
    {
        if (!WeaponReady) return;
        CurrentWeapon.CurrentMode.OnTertiaryInput();
    }
    void OnSelectWeapon(InputValue input)
    {
        if (input.isPressed && weaponSelector.optionsPresent)
        {
            // Run function to open weapon selector
            controller.movement.lookControls.canLook = false;
            int index = SelectorIndexFromWeaponAndMode(equippedWeaponIndex, CurrentWeapon.currentModeIndex);
            Debug.Log("Switching, " + index);
            weaponSelector.EnterMenu(index);
        }
        else
        {
            // Run function to exit weapon selector
            weaponSelector.ExitMenu();
            controller.movement.lookControls.canLook = true;
        }
    }
    void OnLook(InputValue input) => weaponSelector.InputDirection(input.Get<Vector2>(), controller.movement.lookControls.usingGamepad == false);
    void OnScrollWeapon(InputValue input)
    {
        if (isSwitching) return; // Wait until any previous switch operation has finished
        if (weaponSelector.menuIsOpen) return; // Don't allow any other kinds of selection if the radial menu is open
        
        float inputValue = input.Get<float>();
        if (inputValue == 0) return; // If there's no input, do nothing

        int increment = Mathf.RoundToInt(Mathf.Sign(inputValue));

        // Switch either your weapon, or the firing mode on your current weapon
        if (quickSwitchModes)
        {
            int newIndex = MiscFunctions.LoopIndex(CurrentWeapon.currentModeIndex + increment, CurrentWeapon.modes.Length);
            StartCoroutine(CurrentWeapon.SwitchMode(newIndex));
        }
        else
        {
            int newIndex = MiscFunctions.LoopIndex(equippedWeaponIndex + increment, equippedWeapons.Length);
            StartCoroutine(SwitchWeapon(newIndex));
        }
    }
    #endregion

    #region Weapon switching
    void UpdateAvailableWeapons()
    {
        equippedWeapons = GetComponentsInChildren<Weapon>(true);

        foreach (Weapon w in equippedWeapons)
        {
            // Assign proper transform values
            w.transform.SetParent(holdingSocket);
            w.transform.localPosition = Vector3.zero;
            w.transform.localRotation = Quaternion.identity;

            // Disable all weapons so that only the appropriate one will be enabled
            w.gameObject.SetActive(false);
        }
        
        selectorInfo.Setup(this);
        
        if (CurrentWeapon != null)
        {
            StartCoroutine(SwitchWeapon(equippedWeaponIndex));
        }
    }
    public void SetCurrentWeaponActive(bool drawn)
    {
        // If the desired state is already met, do nothing
        if (weaponDrawn == drawn) return;
        // If there's no weapon to draw, do nothing
        if (drawn == true && CurrentWeapon == null) return;
        // If a weapon is active, holster it
        // If a current weapon is selected but not active, draw it
        StartCoroutine(drawn ? Draw() : Holster());
    }
    IEnumerator Holster()
    {
        onHolster.Invoke(CurrentWeapon);
        yield return CurrentWeapon.Holster();
    }
    IEnumerator Draw()
    {
        // Switch index to the new weapon and draw it
        onDraw.Invoke(CurrentWeapon);
        yield return CurrentWeapon.Draw();
    }
    IEnumerator SwitchWeapon(int newIndex)
    {
        if (equippedWeapons.Length <= 0) yield break;
        if (isSwitching) yield break; // Don't attempt another switch if in the middle of another switch operation

        newIndex = Mathf.Clamp(newIndex, 0, equippedWeapons.Length - 1);

        if (weaponDrawn)
        {
            // Do nothing if the desired weapon is already active, or the current weapon is in the middle of another task
            if (equippedWeapons[newIndex] == CurrentWeapon) yield break;
            if (CurrentWeapon.InAction) yield break;
        }

        isSwitching = true;
        onSwitchWeapon.Invoke(equippedWeapons[newIndex]);

        // Wait for the current weapon to be holstered
        if (weaponDrawn) yield return Holster();
        // Switch index to the new weapon
        equippedWeaponIndex = newIndex;
        // Wait while new weapon draws
        yield return Draw();

        isSwitching = false;
    }
    IEnumerator SwitchWeaponAndFiringMode(int weaponIndex, int firingModeIndex)
    {
        yield return SwitchWeapon(weaponIndex);
        yield return CurrentWeapon.SwitchMode(firingModeIndex);
    }
    public void SwitchWeaponAndModeFromIndex(int index)
    {
        GetWeaponAndModeFromSelector(index, out int weaponIndex, out int firingModeIndex);
        StartCoroutine(SwitchWeaponAndFiringMode(weaponIndex, firingModeIndex));
    }
    public void GetWeaponAndModeFromSelector(int index, out int weaponIndex, out int firingModeIndex)
    {
        weaponIndex = 0;
        firingModeIndex = 0;
        for (int i = 0; i < index; i++)
        {
            firingModeIndex++;
            if (firingModeIndex >= equippedWeapons[weaponIndex].modes.Length)
            {
                weaponIndex++;
                firingModeIndex = 0;
            }
        }
    }
    public int SelectorIndexFromWeaponAndMode(int weaponIndex, int firingModeIndex)
    {
        int index = 0;
        for (int w = 0; w < equippedWeapons.Length; w++)
        {
            for (int m = 0; m < equippedWeapons[w].modes.Length; m++)
            {
                if (w == weaponIndex && m == firingModeIndex)
                {
                    return index;
                }
                index++;
            }
            // Calculate where to put borders and weapon graphics
        }
        return 0;
    }
    #endregion
}