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
    [SerializeField] int equippedWeaponIndex;
    public Transform holdingSocket;
    public AmmunitionInventory ammo;
    public RadialMenu weaponSelector;
    public WeaponSelectorHUD selectorInfo;

    [Header("Stats")]
    public Transform aimAxis;
    public float standingAccuracy = 1;
    public float swaySpeed = 0.5f;
    public bool toggleADS;

    [Header("Other")]
    public UnityEvent<Weapon> onDraw;
    public UnityEvent<Weapon> onHolster;

    bool s = false;

    public bool PrimaryHeld { get; private set; }
    public bool SecondaryActive { get; private set; }
    public bool isSwitching { get; private set; }

    public Weapon CurrentWeapon => (equippedWeapons.Length > 0) ? equippedWeapons[equippedWeaponIndex] : null;
    /// <summary>
    /// The direction the player is currently aiming in, accounting for accuracy sway.
    /// </summary>
    public Vector3 AimDirection
    {
        get
        {
            Quaternion sway = AIAim.AimSway(aimSwayAngle, swaySpeed);
            return aimAxis.rotation * sway * Vector3.forward;
        }
    }
    public float aimSwayAngle
    {
        get
        {
            float totalSway = standingAccuracy;
            if (CurrentWeapon.CurrentMode is RangedAttack ra)
            {
                totalSway += ra.stats.sway;
            }
            return totalSway;
        }
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
            // If player is not in the middle of switching weapons
            // If player has a weapon equipped
            // If player is not in the middle of switching firing modes
            // If player is not in weapon wheel
            return enabled && isSwitching == false && CurrentWeapon != null && CurrentWeapon.isSwitching == false/* && weaponSelector.active == false*/;
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
        PrimaryHeld = input.isPressed;
        CurrentWeapon.CurrentMode.OnPrimaryInputChanged();
    }
    void OnADS(InputValue input)
    {
        if (!WeaponReady) return;

        if (toggleADS)
        {
            if (input.isPressed) SecondaryActive = !SecondaryActive;
        }
        else
        {
            SecondaryActive = input.isPressed;
        }
        CurrentWeapon.CurrentMode.OnSecondaryInputChanged();
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
    void OnLook(InputValue input)
    {
        weaponSelector.InputDirection(input.Get<Vector2>());
    }
    public void CancelInputs()
    {
        PrimaryHeld = false;
        CurrentWeapon.CurrentMode.OnPrimaryInputChanged();
        SecondaryActive = false;
        CurrentWeapon.CurrentMode.OnSecondaryInputChanged();
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
    IEnumerator SwitchWeapon(int newIndex)
    {
        if (equippedWeapons.Length <= 0) yield break;

        newIndex = Mathf.Clamp(newIndex, 0, equippedWeapons.Length - 1);
        if (equippedWeapons[newIndex] == CurrentWeapon && equippedWeapons[newIndex].gameObject.activeSelf == true)
        {
            // If selected weapon is already active, no need to run any other code
            yield break;
        }

        // If another weapon is already enabled
        if (CurrentWeapon != null && CurrentWeapon.gameObject.activeSelf == true)
        {
            if (CurrentWeapon.InAction) // If weapon is currently doing something, end this function
            {
                Debug.Log("Switch failed due to being in action on frame " + Time.frameCount);
                yield break;
            }

            isSwitching = true;
            onHolster.Invoke(CurrentWeapon);
            StartCoroutine(CurrentWeapon.Holster());
            //Debug.DrawRay(aimAxis.position, -aimAxis.right, Color.red, CurrentWeapon.switchSpeed);
            yield return new WaitUntil(() => CurrentWeapon.InAction == false);
        }

        isSwitching = true;
        // Once previous weapon is holstered, switch index to the new weapon and draw it
        equippedWeaponIndex = newIndex;
        onDraw.Invoke(CurrentWeapon);
        StartCoroutine(CurrentWeapon.Draw());
        //Debug.DrawRay(aimAxis.position, aimAxis.right, Color.green, CurrentWeapon.switchSpeed);
        yield return new WaitUntil(() => CurrentWeapon.isSwitching == false);

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