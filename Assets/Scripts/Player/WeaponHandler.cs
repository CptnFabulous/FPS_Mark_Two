using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WeaponHandler : MonoBehaviour
{
    public Player controller;
    public SingleInput primaryInput;
    public SingleInput secondaryInput;

    [Header("Weapons")]
    public List<Weapon> equippedWeapons;
    public OffhandAttackHandler offhandAttacks;

    [Header("Stats")]
    public AmmunitionInventory ammo;
    public AimSwayHandler swayHandler;

    [Header("Accessibility")]
    public ADSHandler adsHandler;
    public Transform holdingSocket;
    public RadialMenu weaponSelector;
    public WeaponSelectorHUD selectorInfo;
    public bool toggleADS;
    public bool quickSwitchModes = true;

    [Header("Events")]
    public UnityEvent<Weapon> onDraw;
    public UnityEvent<Weapon> onHolster;
    public UnityEvent<Weapon> onSwitchWeapon;

    int equippedWeaponIndex = 0;

    public bool disableADS { get; set; }
    public bool isSwitching { get; private set; }

    public Weapon CurrentWeapon => (equippedWeapons.Count > 0) ? equippedWeapons[equippedWeaponIndex] : null;
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


    private void Awake()
    {
        if (ammo == null) ammo = GetComponent<AmmunitionInventory>();
        
        weaponSelector.onValueConfirmed.AddListener(SwitchWeaponAndModeFromIndex);
        // Make it so the current weapon is automatically put away if the player dies
        controller.health.onDeath.AddListener((_) => weaponDrawn = false);
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
        CurrentWeapon.CurrentMode.SetPrimaryInput(input.isPressed);
    }
    void OnADS(InputValue input)
    {
        if (!WeaponReady) return;
        CurrentWeapon.CurrentMode.SetSecondaryInput(input.isPressed);
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
        if (quickSwitchModes && CurrentWeapon != null)
        {
            int newIndex = MiscFunctions.LoopIndex(CurrentWeapon.currentModeIndex + increment, CurrentWeapon.modes.Length);
            StartCoroutine(CurrentWeapon.SwitchMode(newIndex));
        }
        else if (equippedWeapons.Count > 0) // Don't allow switching if there's nothing to switch to
        {
            int newIndex = MiscFunctions.LoopIndex(equippedWeaponIndex + increment, equippedWeapons.Count);
            StartCoroutine(SwitchWeapon(newIndex));
        }
    }
    #endregion

    #region Weapon switching
    void UpdateAvailableWeapons()
    {
        equippedWeapons.Clear();
        foreach (Weapon w in GetComponentsInChildren<Weapon>(true))
        {
            AddWeapon(w, false);
        }

        //selectorInfo.Refresh(this);
        
        if (CurrentWeapon != null) StartCoroutine(SwitchWeapon(equippedWeaponIndex));
    }


    public void AddWeapon(Weapon w, bool autoSwitch)
    {
        equippedWeapons.Add(w);

        // Assign proper transform values
        w.transform.SetParent(holdingSocket);
        w.transform.localPosition = Vector3.zero;
        w.transform.localRotation = Quaternion.identity;
        // Pre-emptively disable weapon object so that switching and setup can play properly
        w.gameObject.SetActive(false);
        // Refresh weapon selector
        selectorInfo.Refresh(this);

        // Switch to new weapon, if specified
        if (autoSwitch)
        {
            // Switch to first firing mode
            int index = equippedWeapons.IndexOf(w);
            StartCoroutine(SwitchWeaponAndFiringMode(index, 0));
        }
    }


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
        //(drawn ? onDraw : onHolster).Invoke(CurrentWeapon);
        //yield return (drawn ? CurrentWeapon.Draw() : CurrentWeapon.Holster());
    }
    IEnumerator SwitchWeapon(int newIndex)
    {
        if (equippedWeapons.Count <= 0) yield break;
        if (isSwitching) yield break; // Don't attempt another switch if in the middle of another switch operation

        newIndex = Mathf.Clamp(newIndex, 0, equippedWeapons.Count - 1);

        if (weaponDrawn)
        {
            // Do nothing if the desired weapon is already active, or the current weapon is in the middle of another task
            if (equippedWeapons[newIndex] == CurrentWeapon) yield break;
            if (CurrentWeapon.InAction) yield break;
        }

        isSwitching = true;
        onSwitchWeapon.Invoke(equippedWeapons[newIndex]);
        
        yield return SetCurrentWeaponDrawn(false); // Wait to holster current weapon
        equippedWeaponIndex = newIndex; // Switch to new weapon index
        yield return SetCurrentWeaponDrawn(true); // Wait to draw new weapon

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
        for (int w = 0; w < equippedWeapons.Count; w++)
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