using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
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
            if (isSwitching)
            {
                return true;
            }
            if (CurrentMode.InAction)
            {
                return true;
            }
            return false;
        }
    }

    [HideInInspector] public WeaponHandler user;

    private void OnEnable()
    {
        user = GetComponentInParent<WeaponHandler>();
    }

    public IEnumerator Draw()
    {
        //yield return new WaitUntil(() => isSwitching == false);
        isSwitching = true;
        gameObject.SetActive(true);
        onDraw.Invoke();

        yield return new WaitForSeconds(switchSpeed);

        isSwitching = false;
    }
    public IEnumerator Holster()
    {
        yield return new WaitUntil(() => InAction == false);

        isSwitching = true;
        onHolster.Invoke();

        yield return new WaitForSeconds(switchSpeed);

        gameObject.SetActive(false);
        isSwitching = false;
    }
    public IEnumerator SwitchMode(int newModeIndex)
    {
        if (InAction == true)
        {
            yield break;
        }

        isSwitching = true;
        modes[newModeIndex].onSwitch.Invoke();

        yield return new WaitForSeconds(modes[newModeIndex].switchSpeed);

        currentModeIndex = newModeIndex;
        isSwitching = false;
    }
}