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

    public override string hudInfo
    {
        get
        {
            AmmunitionInventory ammoInv = User.weaponHandler.ammo;
            int totalAmmo = Mathf.RoundToInt(ammoInv.GetValues(ammunitionType).current);
            return $"{totalAmmo}";
        }
    }
    void Awake()
    {
        throwablePrefab.enabled = false;
        throwablePrefab.gameObject.SetActive(false);
    }

    protected override void OnSecondaryInputChanged(bool held) { }
    public override void OnTertiaryInput() { }

    public override bool CanAttack() => User.weaponHandler.ammo.GetStock(ammunitionType) > 0;
    public override void OnAttack() => User.weaponHandler.ammo.Spend(ammunitionType, 1);
    protected override void OnDisable()
    {
        base.OnDisable();

        throwHandler.CancelThrow();

        // If disabled before object was thrown, disable it.
        // It should only offically be released if the throw operation finishes
        if (readyToThrow != null)
        {
            readyToThrow.gameObject.SetActive(false);
            readyToThrow.transform.SetParent(transform);
            readyToThrow.transform.position = throwHandler.hand.transform.position;
        }
    }

    protected override IEnumerator AttackSequence()
    {
        SpawnNewThrowable();
        
        // Prepare to throw the object
        yield return throwHandler.Throw(() => PrimaryHeld);

        // Prep and throw the object, and clear 'readyToThrow'
        readyToThrow.enabled = true;
        readyToThrow.OnThrow();
        readyToThrow = null;
        OnAttack();

        yield return new WaitForSeconds(cooldown);

        //SpawnNewThrowable();

        currentAttack = null;
    }

    void SpawnNewThrowable()
    {
        // Assign an object in 'readyToThrow' if ammunition is present (disable otherwise)
        if (readyToThrow == null)
        {
            readyToThrow = Instantiate(throwablePrefab, throwHandler.hand);
        }
        throwHandler.Pickup(readyToThrow.rb);
        readyToThrow.enabled = false;
        //readyToThrow.gameObject.SetActive(false);
        readyToThrow.gameObject.SetActive(true);
    }
}