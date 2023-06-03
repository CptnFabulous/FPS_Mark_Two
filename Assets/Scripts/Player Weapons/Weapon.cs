using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    public Sprite hudGraphic;
    
    [Header("Attack modes")]
    public WeaponMode[] modes;
    public int currentModeIndex;
    public WeaponMode CurrentMode
    {
        get
        {
            if (modes.Length < 1)
            {
                modes = new WeaponMode[1];
            }
            return modes[currentModeIndex];
        }
    }

    [Header("Switching")]
    public float switchSpeed;
    public UnityEvent onDraw;
    public UnityEvent onHolster;
    public bool isSwitching { get; private set; }
    public bool InAction
    {
        get
        {
            if (isSwitching) return true;
            if (CurrentMode.InAction) return true;
            return false;
        }
    }

    public WeaponHandler user { get; private set; }

    private void OnEnable()
    {
        user = GetComponentInParent<WeaponHandler>();
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

        /*
        isSwitching = true;
        CurrentMode.OnSwitchFrom();
        modes[newModeIndex].onSwitch.Invoke();

        yield return new WaitForSeconds(modes[newModeIndex].switchSpeed);

        currentModeIndex = newModeIndex;
        CurrentMode.OnSwitchTo();
        isSwitching = false;
        */
    }
}