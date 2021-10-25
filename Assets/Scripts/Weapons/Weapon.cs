using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //WeaponHandler playerHolding;


    public WeaponMode[] modes;
    public int currentModeIndex;
    public WeaponMode CurrentMode
    {
        get
        {
            return modes[currentModeIndex];
        }
    }

    public float switchSpeed;
    WaitForSeconds switchTime;

    private void Awake()
    {
        switchTime = new WaitForSeconds(switchSpeed);
    }



    IEnumerator Attack(WeaponHandler user)
    {
        return modes[currentModeIndex].Attack(user);
    }


    IEnumerator Draw()
    {
        gameObject.SetActive(true);
        yield return switchTime;
    }

    IEnumerator Holster()
    {
        yield return switchTime;
        gameObject.SetActive(false);
    }

    IEnumerator SwitchMode(int modeIndex)
    {

        yield return new WaitForSeconds(modes[modeIndex].switchSpeed);

    }

}

/*
public interface PlayerAction : IEnumerator
{
    public int priorityOrder;
    public void Override()
    {

    }
}
*/
/*
public abstract struct PlayerAction
{
    public int priorityOrder;
    public abstract IEnumerator Action();
    public abstract void Cancel();
}
*/
