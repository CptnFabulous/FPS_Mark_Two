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
    public WeaponMode[] modes;
    public int currentModeIndex;

    [Header("Switching")]
    public float switchSpeed;
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
            return _user ??= GetComponentInParent<Character>();
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

    public IEnumerator Draw()
    {
        // Do necessary stuff to disable mode (but don't switch away from it)
        //yield return new WaitUntil(() => isSwitching == false);
        CurrentMode.enabled = false;

        isSwitching = true;

        gameObject.SetActive(true);
        onDraw.Invoke();

        // Wait to switch
        yield return new WaitForSeconds(switchSpeed);

        // Enable current mode so it does all the necessary stuff it needs to do when first activated
        CurrentMode.enabled = true;

        isSwitching = false;
    }
    public IEnumerator Holster()
    {
        //yield return new WaitUntil(() => InAction == false);
        //if (InAction) yield break;
        
        isSwitching = true;

        // Do necessary stuff to disable mode (but don't switch away from it)
        CurrentMode.enabled = false;

        onHolster.Invoke();
        yield return new WaitForSeconds(switchSpeed);

        gameObject.SetActive(false);
        isSwitching = false;
    }
    public IEnumerator SwitchMode(int newModeIndex)
    {
        if (InAction == true) yield break;
        if (newModeIndex == currentModeIndex) yield break;

        isSwitching = true;

        // End current mode
        yield return CurrentMode.SwitchFrom();
        CurrentMode.enabled = false;

        // Officially switch modes
        currentModeIndex = newModeIndex;
        CurrentMode.onSwitch.Invoke();

        // Switch to new mode
        yield return CurrentMode.SwitchTo();
        CurrentMode.enabled = true;

        isSwitching = false;
    }
}