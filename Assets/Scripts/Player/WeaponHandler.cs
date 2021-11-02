using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponHandler : MonoBehaviour
{
    [HideInInspector] public Player controller;
    
    [Header("Weapons")]
    public Weapon[] equippedWeapons;
    int equippedWeaponIndex;
    public Weapon HeldWeapon
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
    public Transform holdingPosition;
    public AmmunitionInventory ammo;

    [Header("Stats")]
    public Transform aimAxis;
    public float standingAccuracy = 1;
    public float swaySpeed = 0.5f;
    
    [Header("Other")]
    public UnityEvent<Weapon> onDraw;
    public UnityEvent<Weapon> onHolster;
    public UnityEvent<WeaponMode> onModeSwitch;
    public UnityEvent<WeaponMode> onAttack;
    

    /// <summary>
    /// The direction the player is currently aiming in, accounting for accuracy sway.
    /// </summary>
    public Vector3 AimDirection()
    {
        float totalSway = standingAccuracy;
        if (HeldWeapon.CurrentMode as RangedAttack != null)
        {
            totalSway += (HeldWeapon.CurrentMode as RangedAttack).stats.sway;
        }
        // Generates changing values from noise
        float noiseX = Mathf.PerlinNoise(Time.time * swaySpeed, 0);
        float noiseY = Mathf.PerlinNoise(0, Time.time * swaySpeed);
        // Converts values from 0 - 1 to -1 - 1
        Vector2 angles = new Vector2(noiseX - 0.5f, noiseY - 0.5f) * 2;
        if (angles.magnitude > 1)
        {
            angles.Normalize();
        }
        angles *= totalSway; //  Multiplies by accuracy value
        // Creates euler angles and combines with current aim direction
        return aimAxis.transform.rotation * Quaternion.Euler(angles.y, angles.x, 0) * Vector3.forward;
        /*
        Quaternion angles = Quaternion.Euler(noiseX * totalSway, 0, noiseY * 360f);
        return aimOrigin.transform.rotation * angles * Vector3.forward;
        */
    }

    private void Awake()
    {
        if (ammo == null)
        {
            ammo = GetComponent<AmmunitionInventory>();
        }
    }
    private void Start()
    {
        UpdateAvailableWeapons();
    }
    private void Update()
    {
        // If player is not in the middle of switching weapons
        // If player has a weapon equipped
        // If player is not in the middle of switching firing modes
        if (isSwitching == false && HeldWeapon != null && HeldWeapon.isSwitchingMode == false)
        {
            HeldWeapon.CurrentMode.UpdateLoop(this);
        }
        
        


    }


    
    
    


    void UpdateAvailableWeapons()
    {
        equippedWeapons = GetComponentsInChildren<Weapon>();
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            equippedWeapons[i].gameObject.SetActive(false);
        }

        if (HeldWeapon != null)
        {
            Debug.Log("Drawing first weapon");
            StartCoroutine(HeldWeapon.Draw(this));
        }
    }

}