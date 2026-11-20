using CptnFabulous.MiscUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AngleCheck
{
    public struct ResultData<T>
    {
        public T returnedValue;
        public RaycastHit hitData;
        public int colliderCount;
        public int successIndex;
    }


    public delegate bool ColliderCheck<T>(Collider collider, out T component);
    
    static Collider[] colliderArray = new Collider[64];
    static GenericComparer<Collider> comparer = new GenericComparer<Collider>(null, false);

    public static bool CheckForObjectsInCone<T>(Vector3 origin, Vector3 direction, float maxAngle, float maxRange, LayerMask layerMask, out T returnedValue, out RaycastHit hitData, ColliderCheck<T> criteria)
    {
        bool success = CheckForObjectsInCone(new Ray(origin, direction), maxAngle, maxRange, layerMask, colliderArray, out ResultData<T> results, criteria);

        returnedValue = results.returnedValue;
        hitData = results.hitData;

        return success;
    }
    /// <summary>
    /// Checks for desired objects in a cone. This code is much simpler than the FieldOfView checks, as it only uses the centre of the collider, so this is more intended for player actions like aim assist and interaction checks.
    /// </summary>
    /// <param name="hitData">Returns more detailed hit data.</param>
    /// <param name="criteria">Weeds out objects that don't meet the desired criteria.</param>
    /// <returns></returns>
    public static bool CheckForObjectsInCone<T>(Ray ray, float maxAngle, float maxRange, LayerMask layerMask, Collider[] colliderArray, out ResultData<T> results, ColliderCheck<T> criteria)
    {
        // Performs different raycasts by changing just one variable, but keeping everything else the same.
        bool InteractionCast(Vector3 dir, out RaycastHit rh) => Physics.Raycast(ray.origin, dir, out rh, maxRange, layerMask);

        // Sorts by both angle and range, so if two targets have very similar angles but one is much closer (or vice versa), it knows how to prioritise them.
        float SorterComparable(Collider c)
        {
            Vector3 point = c.bounds.center;
            float angle = Vector3.Angle(ray.direction, point - ray.origin) / maxAngle;
            float distance = Vector3.Distance(point, ray.origin) / maxRange;
            return angle * distance;
        }

        // Populate default values
        results = new ResultData<T>();
        results.colliderCount = 0;
        results.successIndex = -1;
        results.hitData = new RaycastHit();
        results.returnedValue = default;

        // Perform an initial cast, to prioritise whatever the player is directly aiming at
        bool directCast = InteractionCast(ray.direction, out results.hitData) && criteria.Invoke(results.hitData.collider, out results.returnedValue);
        if (directCast)
        {
            colliderArray[0] = results.hitData.collider;
            results.colliderCount = 1;
            results.successIndex = 0;
            return true;
        }

        // If that didn't return anything, find all colliders within the desired range
        results.colliderCount = Physics.OverlapSphereNonAlloc(ray.origin, maxRange, colliderArray, layerMask);

        // Sort by angle and distance
        comparer.obtainValue = (c) => SorterComparable(c);
        System.Array.Sort(colliderArray, 0, results.colliderCount, comparer);

        // Iterate through colliders
        for (int i = 0; i < results.colliderCount; i++)
        {
            Collider c = colliderArray[i];

            // Check if it's outside the maximum allowed angle. If so, cancel the function because the sorting means we've already gone through the acceptable values.
            Vector3 directionToTarget = c.bounds.center - ray.origin;
            float checkedAngle = Vector3.Angle(ray.direction, directionToTarget);
            if (checkedAngle > maxAngle)
            {
                break;
            }

            // Check that the right object is found with the desired criteria
            if (criteria.Invoke(c, out results.returnedValue) == false) continue;

            // Check line of sight
            if (InteractionCast(directionToTarget, out results.hitData) == false) continue;
            if (results.hitData.collider != c) continue;

            // Cancel loop, we've found an interactable!
            results.successIndex = i;
            return true;
        }

        results.hitData = new RaycastHit();
        results.returnedValue = default;
        return false;
    }
}