using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowHandler : MonoBehaviour
{
    [Header("Throwing stats")]
    public float startingVelocity = 50;
    public float range = 50;
    public float delayBeforeLaunch = 0.25f;
    public float cooldown = 0.5f;

    public Transform hand;

    public Rigidbody holding { get; private set; } = null;
    public bool currentlyThrowing { get; private set; } = false;
    //Coroutine throwCoroutine;

    public Character user;
    public LayerMask attackMask => MiscFunctions.GetPhysicsLayerMask(holding.gameObject.layer);
    public bool InAction => currentlyThrowing;

    private void Awake()
    {
        // If the character dies, they should drop whatever they're holding
        user.health.onDeath.AddListener((__) => Drop(out _));
    }

    public void Pickup(Rigidbody toThrow)
    {
        // Assign reference (and drop the previously held item if there is one)
        Drop(out _);
        holding = toThrow;

        // Disable physics while picked up
        holding.detectCollisions = false;
        holding.isKinematic = true;
        // Later on I might have a different thing to create a physics joint if the object is much larger)

        // Orient object relative to throw socket
        holding.transform.SetParent(hand);
        holding.transform.localPosition = Vector3.zero;
        holding.transform.localRotation = Quaternion.identity;

        // Assign an object in 'readyToThrow' if ammunition is present (disable otherwise)
    }
    public bool Drop(out Rigidbody detached)
    {
        detached = holding;
        holding = null;
        if (detached == null) return false;

        // Separate item from player
        detached.transform.SetParent(null, true);
        // Re-enable movement and collisions
        detached.isKinematic = false;
        detached.detectCollisions = true;

        //detached.velocity = user.rigidbody.velocity;

        return true;
    }
    public void Throw()
    {
        if (holding == null) return;

        currentlyThrowing = true;

        // Prep to throw by calculating direction
        Vector3 aimOrigin = user.LookTransform.position;
        Vector3 aimDirection = user.LookTransform.forward;
        Vector3 throwOrigin = holding.transform.position;
        WeaponUtility.CalculateObjectLaunch(aimOrigin, throwOrigin, aimDirection, range, attackMask, user.colliders, out Vector3 throwDirection, out _, out _, out _);
        throwDirection = throwDirection.normalized;
        Debug.DrawRay(throwOrigin, throwDirection * range, Color.blue, 5);

        // Detach object and apply velocity
        Drop(out Rigidbody toThrow);
        toThrow.AddForce(throwDirection * startingVelocity, ForceMode.Impulse); 
        currentlyThrowing = false;
    }
}