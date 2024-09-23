using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    public Entity parentEntity;
    public Sprite hudGraphic;
    
    [Header("Attack modes")]
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
            if (CurrentMode.InAction) return true;
            return false;
        }
    }

    public IEnumerator Draw()
    {
        //yield return new WaitUntil(() => isSwitching == false);
        CurrentMode.OnSwitchFrom();
        isSwitching = true;
        gameObject.SetActive(true);
        onDraw.Invoke();

        yield return new WaitForSeconds(switchSpeed);

        CurrentMode.OnSwitchTo();
        isSwitching = false;
    }
    public IEnumerator Holster()
    {
        //yield return new WaitUntil(() => InAction == false);
        if (InAction == true)
        {
            yield break;
        }

        isSwitching = true;
        CurrentMode.OnSwitchFrom();
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
        CurrentMode.OnSwitchFrom();
        currentModeIndex = newModeIndex;
        CurrentMode.onSwitch.Invoke();

        yield return new WaitForSeconds(CurrentMode.switchSpeed);

        CurrentMode.OnSwitchTo();
        isSwitching = false;
    }
}