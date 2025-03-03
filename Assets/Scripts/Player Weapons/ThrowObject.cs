using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowObject : WeaponMode
{
    public AmmunitionType ammunitionType;
    public ThrowHandler throwHandler;

    [Header("Stats")]
    public Throwable throwablePrefab;
    //[SerializeField] float startingVelocity = 50;
    //[SerializeField] float range = 50;
    //[SerializeField] float delayBeforeLaunch = 0.25f;
    [SerializeField] float cooldown = 0.5f;

    Throwable readyToThrow;

    public override LayerMask attackMask => throwHandler.attackMask;

    void Awake()
    {
        throwablePrefab.enabled = false;
        throwablePrefab.gameObject.SetActive(false);
    }

    protected override void OnSecondaryInputChanged(bool held) { }
    public override void OnTertiaryInput() { }

    public override bool CanAttack() => User.weaponHandler.ammo.GetStock(ammunitionType) > 0;
    public override void OnAttack() => User.weaponHandler.ammo.Spend(ammunitionType, 1);

    protected override IEnumerator AttackSequence()
    {
        SpawnNewThrowable();
        
        Debug.Log($"{this}: throwing");
        // Prep and throw the object, and clear 'readyToThrow'
        readyToThrow.enabled = true;
        throwHandler.Throw();
        readyToThrow.OnThrow();
        readyToThrow = null;
        OnAttack();

        yield return new WaitForSeconds(cooldown);

        //SpawnNewThrowable();

        currentAttack = null;
        yield break;
    }

    void SpawnNewThrowable()
    {
        // Assign an object in 'readyToThrow' if ammunition is present (disable otherwise)
        if (readyToThrow == null)
        {
            readyToThrow = Instantiate(throwablePrefab, throwHandler.hand);
            throwHandler.Pickup(readyToThrow.rb);
        }
        readyToThrow.enabled = false;
        readyToThrow.gameObject.SetActive(true);
    }
    
    public override void OnSwitchTo()
    {
        //throw new System.NotImplementedException();
    }

    public override void OnSwitchFrom()
    {
        //throw new System.NotImplementedException();
    }
}