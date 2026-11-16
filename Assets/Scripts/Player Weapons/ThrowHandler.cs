using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrowHandler : MonoBehaviour
{
    public Character user;
    public Transform hand;
    public TrajectoryRenderer arcRenderer;

    [Header("Throwing stats")]
    public float startForce = 300;
    public float startTravelSpeed = 12;
    public float range = 50;
    public float delayBeforeLaunch = 0.25f;
    public float cooldown = 0.5f;
    public bool accountForMass = false;

    public UnityEvent<Rigidbody> onPickup;
    public UnityEvent<Rigidbody> onDrop;
    public UnityEvent<Rigidbody> onThrow;


    public Rigidbody holding { get; private set; } = null;
    public bool currentlyThrowing { get; private set; } = false;

    public LayerMask attackMask => PhysicsUtility.GetPhysicsLayerMask(holding.gameObject.layer);

    private void Awake()
    {
        // If the character dies, they should drop whatever they're holding
        user.health.onDeath.AddListener((__) => Drop(out _));
    }

    public IEnumerator PickupSequence(Rigidbody toPickUp, float pickupTime)
    {
        // Assign reference (and drop the previously held item if there is one)
        Drop(out _);
        holding = toPickUp;

        // Disable physics while picked up
        SetPhysicsInteractions(holding, false);
        // Later on I might have a different thing to create a physics joint if the object is much larger)

        // Orient object relative to throw socket
        holding.transform.SetParent(hand);

        onPickup.Invoke(toPickUp);

        Vector3 startPosition = holding.transform.localPosition;
        Quaternion startRotation = holding.transform.localRotation;

        float timer = 0;
        while (timer < 1)
        {
            // Cancel function if item was dropped before picking-up animation finished
            if (holding == null) yield break;
            
            //Debug.Log(timer);
            timer += Time.deltaTime / pickupTime;
            timer = Mathf.Clamp01(timer);

            Vector3 position = -holding.centerOfMass;
            holding.transform.localPosition = Vector3.Lerp(startPosition, position, timer);
            holding.transform.localRotation = Quaternion.Lerp(startRotation, Quaternion.identity, timer);

            yield return null;
        }
    }
    public void Pickup(Rigidbody toPickUp)
    {
        if (toPickUp == holding) return;

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

        onPickup.Invoke(toPickUp);
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

        onDrop.Invoke(detached);

        return true;
    }
    public IEnumerator Throw(System.Func<bool> buttonHoldInput)
    {
        if (holding == null) yield break;

        currentlyThrowing = true;

        #region Wind-up

        // Calculate desired speed (based on limits so objects don't fly stupidly far)
        float totalMass = PhysicsCache.TotalMassOfConnectedRigidbodies(holding);
        CalculateLimitedSpeed(totalMass, startForce, startTravelSpeed, out float force, out float speed, out float reductionRatio);
        float magnitude = accountForMass ? force : speed;

        // Activate trajectory handler, and assign values so it can calculate the correct trajectory
        // (A delegate is set up so we don't have to repeatedly input data from our end)
        arcRenderer.gameObject.SetActive(true);
        arcRenderer.mass = holding.mass;
        arcRenderer.drag = holding.drag;
        arcRenderer.hitDetection = attackMask;
        arcRenderer.getStartPositionAndVelocity = () =>
        {
            CalculateObjectLaunch(out Vector3 origin, out Vector3 direction);
            return (origin, direction * speed);
        };

        // Wait while throw input is held
        yield return new WaitWhile(buttonHoldInput.Invoke);
        
        arcRenderer.gameObject.SetActive(false);

        #endregion

        holding.gameObject.SetActive(true);

        // Calculate origin and direction for final launch
        CalculateObjectLaunch(out Vector3 throwOrigin, out Vector3 throwDirection);
        // Detach object and apply velocity
        Drop(out Rigidbody toThrow);

        // Throw
        AddForceToRigidbodyChain(toThrow, magnitude * throwDirection, throwOrigin, ForceMode.Impulse);
        // Update the last time thrown for the user's health, so the player can't be damaged by an object they just threw due to wacky physics
        user.health.timesPhysicsObjectsWereLaunchedByThisEntity[toThrow.gameObject] = Time.time;

        onThrow.Invoke(toThrow);
        currentlyThrowing = false;
    }
    public void CancelThrow()
    {
        Drop(out _);
        arcRenderer.gameObject.SetActive(false);
    }

    void CalculateObjectLaunch(out Vector3 throwOrigin, out Vector3 throwDirection)
    {
        // Create an exceptions list that accounts for the colliders of both the user and the held item
        // Making a new list for every throw does create garbage, but it's not like this function runs every frame
        List<Collider> exceptions = new List<Collider>(user.colliders);
        exceptions.AddRange(PhysicsCache.GetChildColliders(holding));

        // Calculate directions for initial cast
        Vector3 aimOrigin = user.LookTransform.position;
        Vector3 aimDirection = user.LookTransform.forward;
        throwOrigin = holding.worldCenterOfMass;

        WeaponUtility.CalculateObjectLaunch(aimOrigin, throwOrigin, aimDirection, range, attackMask, ColliderIsException, out throwDirection, out _, out _, out _);
        throwDirection = throwDirection.normalized;
    }

    bool ColliderIsException(RaycastHit rh)
    {
        if (user.HitOwnCollider(rh)) return true;
        if (CollectionUtility.ArrayContains(PhysicsCache.GetChildColliders(holding), rh.collider)) return true;
        return false;
    }

    public static void CalculateLimitedSpeed(float mass, float startForce, float maxSpeed, out float calculatedForce, out float calculatedSpeed, out float reductionRatio)
    {
        calculatedSpeed = startForce / mass;
        reductionRatio = maxSpeed / calculatedSpeed;
        calculatedSpeed = Mathf.Min(calculatedSpeed, maxSpeed);
        calculatedForce = calculatedSpeed * mass;
    }
    public static void AddForceToRigidbodyChain(Rigidbody toThrow, Vector3 force, Vector3 position, ForceMode mode)
    {
        bool accountForMass = mode == ForceMode.Impulse || mode == ForceMode.Force;

        // Calculate total mass
        float totalMass = PhysicsCache.TotalMassOfConnectedRigidbodies(toThrow);
        // Apply force to all child rigidbodies
        foreach (Rigidbody rb in PhysicsCache.GetChildRigidbodies(toThrow))
        {
            // Calculate the fraction of the total mass for each rigidbody, and apply force proportionally
            float fraction = 1;
            if (accountForMass) fraction = rb.mass / totalMass;
            rb.AddForceAtPosition(fraction * force, position, mode);
        }
    }
    public static void SetPhysicsInteractions(Rigidbody target, bool active)
    {
        target.detectCollisions = active;
        target.isKinematic = !active;
        foreach (Rigidbody rb in PhysicsCache.GetChildRigidbodies(target))
        {
            rb.detectCollisions = active;
            //rb.isKinematic = !active;
        }
    }
}
