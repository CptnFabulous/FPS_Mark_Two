using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.SocialPlatforms;

public class ThrowObject : WeaponMode
{
    public AmmunitionType ammunitionType;
    public Throwable throwablePrefab;
    [SerializeField] float startingVelocity = 50;
    [SerializeField] float range = 50;

    [SerializeField] float delayBeforeLaunch = 0.25f;
    [SerializeField] float cooldown = 0.5f;

    public Transform hand;

    Throwable readyToThrow;
    bool currentlyThrowing = false;
    //Coroutine throwCoroutine;

    public override LayerMask attackMask => MiscFunctions.GetPhysicsLayerMask(throwablePrefab.gameObject.layer);
    public override bool InAction => currentlyThrowing;


    void Awake()
    {
        throwablePrefab.enabled = false;
        throwablePrefab.gameObject.SetActive(false);
    }


    protected override void OnSecondaryInputChanged() { }
    public override void OnTertiaryInput() { }

    protected override void OnPrimaryInputChanged(bool held)
    {
        // Check if input is pressed and not released
        if (PrimaryHeld == false) return;

        if (User.weaponHandler.ammo.GetStock(ammunitionType) <= 0) return;

        Debug.Log($"{this}: throwing");

        
        SpawnNewThrowable();

        User.weaponHandler.ammo.Spend(ammunitionType, 1);

        // Prep and throw the object, and clear 'readyToThrow'
        Vector3 aimOrigin = User.LookTransform.position;
        Vector3 aimDirection = User.LookTransform.forward;
        Vector3 throwOrigin = readyToThrow.transform.position;
        int collisionMask = MiscFunctions.GetPhysicsLayerMask(readyToThrow.collider.gameObject.layer);
        WeaponUtility.CalculateObjectLaunch(aimOrigin, throwOrigin, aimDirection, range, collisionMask, User.colliders, out Vector3 throwDirection, out _, out _, out _);
        Debug.DrawRay(throwOrigin, throwDirection * range, Color.blue, 5);
        readyToThrow.TriggerThrow(throwDirection * startingVelocity);
        readyToThrow = null;

        //SpawnNewThrowable();
    }
    
    protected override void OnInterrupt()
    {
        // Stop throw coroutine
        //StopCoroutine(throwCoroutine);
        
    }

    
    void SpawnNewThrowable()
    {
        // Assign an object in 'readyToThrow' if ammunition is present (disable otherwise)
        if (readyToThrow == null)
        {
            readyToThrow = Instantiate(throwablePrefab, hand);
            readyToThrow.transform.localPosition = Vector3.zero;
            readyToThrow.transform.localRotation = Quaternion.identity;
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
