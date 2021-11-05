using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Stats")]
    public Transform aimAxis;
    public float standingAccuracy = 1;
    public float swaySpeed = 0.5f;
    public bool toggleADS;
    public ButtonInput primary = new ButtonInput("Fire");
    public ButtonInput secondary = new ButtonInput("Aim");
    public ButtonInput tertiary = new ButtonInput("Reload");

    [Header("Other")]
    public UnityEvent<Weapon> onDraw;
    public UnityEvent<Weapon> onHolster;
    /*
    public UnityEvent<WeaponMode> onModeSwitch;
    public UnityEvent<WeaponMode> onAttack;
    */

    

    /// <summary>
    /// The direction the player is currently aiming in, accounting for accuracy sway.
    /// </summary>
    public Vector3 AimDirection()
    {
        float totalSway = standingAccuracy;
        if (CurrentWeapon.CurrentMode as RangedAttack != null)
        {
            totalSway += (CurrentWeapon.CurrentMode as RangedAttack).stats.sway;
        }
        // Generates changing values from noise
        float noiseX = Mathf.PerlinNoise(Time.time * swaySpeed, 0);
        float noiseY = Mathf.PerlinNoise(0, Time.time * swaySpeed);
        // Converts values from 0 - 1 to -1 - 1
        Vector2 angles = new Vector2(noiseX - 0.5f, noiseY - 0.5f) * 2;
        /*
        if (angles.magnitude > 1)
        {
            angles.Normalize();
        }
        */
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
        /*
        // If player is not currently 
        if (isSwitching == false && CurrentWeapon.InAction == false)
        {
            float scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAxis != 0)
            {
                int newIndex = equippedWeaponIndex;
                if (scrollAxis > 0)
                {
                    newIndex -= 1;
                }
                else if (scrollAxis < 0)
                {
                    newIndex += 1;
                }
                StartCoroutine(SwitchWeapon(newIndex));
            }
        }
        */
        
        if (MiscFunctions.NumKeyPressed(out int index, true))
        {
            StartCoroutine(SwitchWeapon(index));
        }
        
        // If player is not in the middle of switching weapons
        // If player has a weapon equipped
        // If player is not in the middle of switching firing modes
        if (isSwitching == false && CurrentWeapon != null && CurrentWeapon.isSwitching == false)
        {
            CurrentWeapon.CurrentMode.UpdateLoop(this);
        }
        
        


    }


    
    
    


    void UpdateAvailableWeapons()
    {
        //Weapon current = CurrentWeapon;
        equippedWeapons = GetComponentsInChildren<Weapon>(true);
        for (int i = 0; i < equippedWeapons.Length; i++)
        {
            Debug.Log("Weapon present: " + equippedWeapons[i].name);
            equippedWeapons[i].gameObject.SetActive(false);
        }

        if (CurrentWeapon != null)
        {
            Debug.Log("Drawing first weapon, " + CurrentWeapon.name);
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
            yield return new WaitUntil(() => CurrentWeapon.InAction == false);
        }

        isSwitching = true;
        // Once previous weapon is holstered, switch index to the new weapon and draw it
        equippedWeaponIndex = newIndex;
        onDraw.Invoke(CurrentWeapon);
        StartCoroutine(CurrentWeapon.Draw());
        yield return new WaitUntil(() => CurrentWeapon.isSwitching == false);

        isSwitching = false;
    }
}

public struct ButtonInput
{
    string inputName;

    public bool Pressed
    {
        get
        {
            return Input.GetButtonDown(inputName);
        }
    }
    public bool Held
    {
        get
        {
            return Input.GetButton(inputName);
        }
    }
    public bool Released
    {
        get
        {
            return Input.GetButtonUp(inputName);
        }
    }

    public ButtonInput(string name)
    {
        inputName = name;
    }
}