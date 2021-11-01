using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
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

    public float switchSpeed;
    public UnityEvent onDraw;
    public UnityEvent onHolster;

    [HideInInspector] public bool isSwitchingMode;
    WaitForSeconds switchYield;




    public IEnumerator Draw(WeaponHandler handler)
    {
        if (handler.isSwitching == true)
        {
            yield break;
        }

        handler.isSwitching = true;
        gameObject.SetActive(true);
        onDraw.Invoke();
        yield return switchYield;
        handler.isSwitching = false;
    }
    public IEnumerator Holster(WeaponHandler handler)
    {
        if (handler.isSwitching == true)
        {
            yield break;
        }

        handler.isSwitching = true;
        onHolster.Invoke();
        yield return switchYield;
        gameObject.SetActive(false);
        handler.isSwitching = false;
    }
    public IEnumerator SwitchMode(int newModeIndex)
    {
        if (isSwitchingMode == true)
        {
            yield break;
        }

        isSwitchingMode = true;
        modes[newModeIndex].onSwitch.Invoke();
        yield return new WaitForSeconds(modes[newModeIndex].switchSpeed);
        currentModeIndex = newModeIndex;
        isSwitchingMode = false;
    }



    private void Awake()
    {
        switchYield = new WaitForSeconds(switchSpeed);
    }

}