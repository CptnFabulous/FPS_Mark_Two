using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AngleCheck
{
    static List<Collider> collidersChecked = new List<Collider>();

    public delegate bool ColliderCheck<T>(Collider collider, out T component);
    
    /// <summary>
    /// Checks for desired objects in a cone. This code is much simpler than the FieldOfView checks, as it only uses the centre of the collider, so this is more intended for player actions like aim assist and interaction checks.
    /// </summary>
    /// <param name="hitData">Returns more detailed hit data.</param>
    /// <param name="criteria">Weeds out objects that don't meet the desired criteria.</param>
    /// <returns></returns>
    public static bool CheckForObjectsInCone<T>(Vector3 origin, Vector3 direction, float maxAngle, float range, LayerMask layerMask, out T returnedValue, out RaycastHit hitData, ColliderCheck<T> criteria)
    {
        bool InteractionCast(Vector3 dir, out RaycastHit rh) => Physics.Raycast(origin, dir, out rh, range, layerMask);

        hitData = new RaycastHit();
        returnedValue = default;

        // Perform an initial cast, to prioritise whatever the player is directly aiming at
        bool directCast = InteractionCast(direction, out hitData) && criteria.Invoke(hitData.collider, out returnedValue);
        if (directCast) return true;

        // If that didn't return anything, do a sweep for other objects within the desired range and angle

        // Obtain initial colliders, then sort by the closest angle relative to the aim direction
        // This way we don't have to check the whole list, only until we find one valid option
        collidersChecked.Clear();
        collidersChecked.AddRange(Physics.OverlapSphere(origin, range, layerMask));
        MiscFunctions.SortListWithOnePredicate(collidersChecked, (c) => Vector3.Dot(direction, c.bounds.center - origin), true);

        // Get first entry that meets the criteria
        foreach (Collider c in collidersChecked)
        {
            // Check that the right object is found with the desired criteria
            if (criteria.Invoke(c, out returnedValue) == false) continue;

            // Ensure collider is within angle 
            Vector3 directionToTarget = c.bounds.center - origin;
            float checkedAngle = Vector3.Angle(direction, directionToTarget);
            if (checkedAngle > maxAngle) continue;

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