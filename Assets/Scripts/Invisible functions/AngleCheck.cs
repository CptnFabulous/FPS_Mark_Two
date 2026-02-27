using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AngleCheck
{
    public delegate bool ColliderCheck<T>(Collider collider, out T component);
    
    static Collider[] colliderArray = new Collider[64];
    static GenericComparer<Collider> comparer = new GenericComparer<Collider>(null, false);

    /// <summary>
    /// Checks for desired objects in a cone. This code is much simpler than the FieldOfView checks, as it only uses the centre of the collider, so this is more intended for player actions like aim assist and interaction checks.
    /// </summary>
    /// <param name="hitData">Returns more detailed hit data.</param>
    /// <param name="criteria">Weeds out objects that don't meet the desired criteria.</param>
    /// <returns></returns>
    public static bool CheckForObjectsInCone<T>(Vector3 origin, Vector3 direction, float maxAngle, float range, LayerMask layerMask, out T returnedValue, out RaycastHit hitData, ColliderCheck<T> criteria/*, bool debug = false*/)
    {
        bool InteractionCast(Vector3 dir, out RaycastHit rh) => Physics.Raycast(origin, dir, out rh, range, layerMask);

        // Populate default values
        hitData = new RaycastHit();
        returnedValue = default;

        // Perform an initial cast, to prioritise whatever the player is directly aiming at
        bool directCast = InteractionCast(direction, out hitData) && criteria.Invoke(hitData.collider, out returnedValue);
        if (directCast) return true;

        // If that didn't return anything, do a sweep for other objects within the desired range and angle
        int colliderCount = Physics.OverlapSphereNonAlloc(origin, range, colliderArray, layerMask); // Get colliders
        comparer.obtainValue = (c) => Vector3.Angle(direction, c.bounds.center - origin); // Set up comparer to sort by angle
        System.Array.Sort(colliderArray, 0, colliderCount, comparer); // Do the actual sorting

        // Iterate through colliders
        for (int i = 0; i <  colliderCount; i++)
        {
            Collider c = colliderArray[i];

            // Check if it's outside the maximum allowed angle. If so, cancel the function because the sorting means we've already gone through the acceptable values.
            Vector3 directionToTarget = c.bounds.center - origin;
            float checkedAngle = Vector3.Angle(direction, directionToTarget);
            if (checkedAngle > maxAngle) break;

            // Check that the right object is found with the desired criteria
            if (criteria.Invoke(c, out returnedValue) == false) continue;

            // Check line of sight
            if (InteractionCast(directionToTarget, out hitData) == false) continue;
            if (hitData.collider != c) continue;

            // Cancel loop, we've found an interactable!
            return true;
        }

        hitData = new RaycastHit();
        returnedValue = default;
        return false;
    }
}