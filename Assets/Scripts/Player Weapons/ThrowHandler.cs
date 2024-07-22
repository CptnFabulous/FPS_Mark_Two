using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrowHandler : MonoBehaviour
{
    public Character user;
    public Transform hand;

    [Header("Throwing stats")]
    public float startingVelocity = 50;
    public float range = 50;
    public float delayBeforeLaunch = 0.25f;
    public float cooldown = 0.5f;

    public UnityEvent<Rigidbody> onThrow;

    public Rigidbody holding { get; private set; } = null;
    public bool currentlyThrowing { get; private set; } = false;
    //Coroutine throwCoroutine;

    public LayerMask attackMask => MiscFunctions.GetPhysicsLayerMask(holding.gameObject.layer);
    public bool InAction => currentlyThrowing;

    private void Awake()
    {
        // If the character dies, they should drop whatever they're holding
        user.health.onDeath.AddListener((__) => Drop(out _));
    }

    public void Pickup(Rigidbody toPickUp)
    {
        // Assign reference (and drop the previously held item if there is one)
        Drop(out _);
        holding = toPickUp;

        // Disable physics while picked up
        SetPhysicsInteractions(holding, false);
        // Later on I might have a different thing to create a physics joint if the object is much larger)

        // Orient object relative to throw socket
        holding.transform.SetParent(hand);

        Vector3 position = -holding.centerOfMass;
        holding.transform.localPosition = position;
        holding.transform.localRotation = Quaternion.identity;
    }
    public bool Drop(out Rigidbody detached)
    {
        detached = holding;
        holding = null;
        if (detached == null) return false;

        // Separate item from player
        detached.transform.SetParent(null, true);
        // Re-enable movement and collisions
        SetPhysicsInteractions(detached, true);
        
        // Inherit velocity
        // TO DO: make it distribute velocity between all child rigidbodies
        //detached.velocity = user.rigidbody.velocity;

        return true;
    }
    public void Throw()
    {
        if (holding == null) return;

        currentlyThrowing = true;

        // Create an exceptions list that accounts for the colliders of both the user and the held item
        // Making a new list for every throw does create garbage, but it's not like this function runs every frame
        List<Collider> exceptions = new List<Collider>(user.colliders);
        exceptions.AddRange(PhysicsCache.GetChildColliders(holding));

        // Calculate directions for initial cast
        Vector3 aimOrigin = user.LookTransform.position;
        Vector3 aimDirection = user.LookTransform.forward;
        Vector3 throwOrigin = holding.transform.position;
        // Calculate the direction to throw the object in
        WeaponUtility.CalculateObjectLaunch(aimOrigin, throwOrigin, aimDirection, range, attackMask, exceptions, out Vector3 throwDirection, out _, out _, out _);
        throwDirection = throwDirection.normalized;
        //Debug.DrawRay(throwOrigin, throwDirection * range, Color.blue, 5);

        // Detach object and apply velocity
        Drop(out Rigidbody toThrow);
        //toThrow.AddForce(throwDirection * startingVelocity, ForceMode.Impulse);
        //toThrow.AddForceAtPosition(throwDirection * startingVelocity, throwOrigin, ForceMode.Impulse);
        AddForceToRigidbodyChain(toThrow, throwDirection * startingVelocity, throwOrigin, ForceMode.Impulse);
        // Update the last time thrown for the user's health, so the player can't be damaged by an object they just threw due to wacky physics
        user.health.timesPhysicsObjectsWereLaunchedByThisEntity[toThrow.gameObject] = Time.time;

        onThrow.Invoke(toThrow);

        currentlyThrowing = false;
    }

    public static void AddForceToRigidbodyChain(Rigidbody toThrow, Vector3 force, Vector3 position, ForceMode mode)
    {
        // Calculate total mass
        float totalMass = PhysicsCache.TotalMassOfConnectedRigidbodies(toThrow);
        // Apply force to all child rigidbodies
        foreach (Rigidbody rb in PhysicsCache.GetChildRigidbodies(toThrow))
        {
            // Calculate the fraction of the total mass for each rigidbody, and apply force proportionally
            float fraction = rb.mass / totalMass;
            rb.AddForceAtPosition(force * fraction, position, mode);
        }
    }

    public static void SetPhysicsInteractions(Rigidbody target, bool active)
    {
        target.detectCollisions = active;
        target.isKinematic = !active;
        /*
        foreach (Rigidbody rb in PhysicsCache.GetChildRigidbodies(target))
        {
            rb.detectCollisions = active;
            //rb.isKinematic = !active;
        }
        */
    }
}
