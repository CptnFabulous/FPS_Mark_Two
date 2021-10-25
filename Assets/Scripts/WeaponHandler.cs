using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public Character characterUsing;

    public Transform aimOrigin;

    public Weapon[] equippedWeapons;
    int equippedWeaponIndex;
    IEnumerator currentAction;
    //int currentActionPriority;



    private void Awake()
    {
        UpdateAvailableWeapons();
    }

    void UpdateAvailableWeapons()
    {
        equippedWeapons = GetComponentsInChildren<Weapon>();
    }


    private void Update()
    {
        //StartCoroutine(currentAction);

        /*
        if (AttackButtonPressed && currentAction == null)
        {
            PerformAction(CurrentMode.Attack(this));
        }
        */


    }

    public bool AttackButtonPressed
    {
        get
        {
            return Input.GetButtonDown("Attack");
        }
    }
    public bool AttackButtonHeld
    {
        get
        {
            return Input.GetButton("Attack");
        }
    }


    public Weapon HeldWeapon
    {
        get
        {
            return equippedWeapons[equippedWeaponIndex];
        }
    }


    public void PerformAction(IEnumerator action/*, int priority*/)
    {
        //if (currentAction.)
        
        /*
        if (currentAction != null && priority >= currentActionPriority)
        {
            StopCoroutine(currentAction);
        }
        */
        StopCoroutine(currentAction);

        currentAction = action;
        //currentActionPriority = priority;
        StartCoroutine(currentAction);
    }


    
}

/*
public abstract class PlayerAction
{
    public IEnumerator reference;
    public abstract IEnumerator Action();
    public abstract void Cancel();
    
}
*/

/*
public virtual IEnumerator<WeaponHandler> PlayerAction()
{

}
*/

