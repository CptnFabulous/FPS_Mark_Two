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
    public int equippedWeaponIndex;
    public Weapon CurrentWeapon
    {
        get
        {
            if (equippedWeapons.Length <= 0)
            {
                return null;
            }
            return equippedWeapons[equippedWeaponIndex];
        }
    }
    [HideInInspector] public bool isSwitching;
    public Transform holdingSocket;
    public AmmunitionInventory ammo;
    public RadialMenu weaponSelector;
    public WeaponSelectorHUD selectorInfo;

    [Header("Stats")]
    public Transform aimAxis;
    public float standingAccuracy = 1;
    public float swaySpeed = 0.5f;
    public bool toggleADS;

    /// <summary>
    /// The direction the player is currently aiming in, accounting for accuracy sway.
    /// </summary>
    public Vector3 AimDirection
    {
        get
        {
            float totalSway = standingAccuracy;
            if (CurrentWeapon.CurrentMode as RangedAttack != null)
            {
                totalSway += (CurrentWeapon.CurrentMode as RangedAttack).stats.sway;
            }
            return (aimAxis.transform.rotation * AIAim.AimSway(totalSway, swaySpeed)) * Vector3.forward;
        }
    }

    /// <summary>
    /// Is the player currently in ADS on a particular weapon?
    /// </summary>
    public bool IsUsingADS
    {
        get
        {
            if (CurrentWeapon == null)
            {
                return false;
            }
            RangedAttack r = CurrentWeapon.CurrentMode as RangedAttack;
            return r != null && r.optics != null && r.optics.IsAiming;
        }
    }

    [Header("Other")]
    public UnityEvent<Weapon> onDraw;
    public UnityEvent<Weapon> onHolster;
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
        if (ammo == null)
        {
            ammo = GetComponent<AmmunitionInventory>();
        }
        
        weaponSelector.onValueConfirmed.AddListener(SwitchWeaponAndModeFromIndex);
        weaponSelector.onValueChanged.AddListener((i) =>
        {
            GetWeaponAndModeFromSelector(i, out int weaponIndex, out int firingModeIndex);
            RangedAttack r = equippedWeapons[weaponIndex].modes[firingModeIndex] as RangedAttack;
            if (r != null)
            {
                selectorInfo.Refresh(r);
            }
        });
    }
    private void Start()
    {
        UpdateAvailableWeapons();
    }
    private void Update()
    {
        if (weaponSelector.menuIsOpen == false && NumberKeySelector.NumKeyPressed(out int index, true))
        {
            //StartCoroutine(SwitchWeapon(index));
            SwitchWeaponAndModeFromIndex(index);
        }
    }

    #region Inputs
    void OnFire(InputValue input)
    {
        if (!WeaponReady)
        {
            return;
        }
        PrimaryHeld = input.isPressed;
        CurrentWeapon.CurrentMode.OnPrimaryInputChanged();
    }
    void OnADS(InputValue input)
    {
        if (!WeaponReady)
        {
            return;
        }

        if (toggleADS)
        {
            if (input.isPressed)
            {
                SecondaryActive = !SecondaryActive;
            }
        }
        else
        {
            SecondaryActive = input.isPressed;
        }
        CurrentWeapon.CurrentMode.OnSecondaryInputChanged();
    }
    void OnReload()
    {
        if (!WeaponReady)
        {
            return;
        }
        CurrentWeapon.CurrentMode.OnTertiaryInput();
    }
    void OnSelectWeapon(InputValue input)
    {
        if (input.isPressed && weaponSelector.optionsPresent)
        {
            // Run function to open weapon selector
            controller.movement.canLook = false;
            int index = SelectorIndexFromWeaponAndMode(equippedWeaponIndex, CurrentWeapon.currentModeIndex);
            Debug.Log("Switching, " + index);
            weaponSelector.EnterMenu(index);
        }
        else
        {
            // Run function to exit weapon selector
            controller.movement.canLook = true;
            weaponSelector.ExitMenu();
        }
    }
    void OnLook(InputValue input)
    {
        weaponSelector.InputDirection(input.Get<Vector2>());
    }
    public bool PrimaryHeld { get; private set; }
    public bool SecondaryActive { get; private set; }
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
        if (equippedWeapons.Length <= 0)
        {
            yield break;
        }

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
        StartCoroutine(SwitchWeapon(weaponIndex));
        yield return new WaitWhile(()=> isSwitching);
        CurrentWeapon.StartCoroutine(CurrentWeapon.SwitchMode(firingModeIndex));
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