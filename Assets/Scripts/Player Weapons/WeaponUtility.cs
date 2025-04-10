using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class WeaponUtility
{
    public static void CalculateObjectLaunch(Vector3 aimOrigin, Vector3 launchOrigin, Vector3 direction, float range, LayerMask detection, IList<Collider> exceptions, out Vector3 launchDirection, out Vector3 castHitPoint, out RaycastHit rh, out bool hitPointIsBehindMuzzle)
    {
        bool successfulCast = MiscFunctions.RaycastWithExceptions(aimOrigin, direction, out rh, range, detection, exceptions);
        // Calculate where the projectile needs to go
        castHitPoint = successfulCast ? rh.point : aimOrigin + (direction * range);
        // Calculate the direction the projectile needs to move in
        launchDirection = castHitPoint - launchOrigin;
        // Check the dot product in case the hit point is behind the muzzle (e.g. if the player is shooting into a wall)
        hitPointIsBehindMuzzle = successfulCast && Vector3.Dot(launchDirection, direction) < 0;
    }
    public static string AmmoCounterHUDDisplay(RangedAttack rangedAttack, string infiniteText = "\u221E")
    {
        bool consumesAmmo = rangedAttack.consumesAmmo;
        int totalAmmo = Mathf.RoundToInt(rangedAttack.User.weaponHandler.ammo.GetValues(rangedAttack.stats.ammoType).current);
        
        if (rangedAttack.magazine != null)
        {
            int ammoInMagazine = Mathf.RoundToInt(rangedAttack.magazine.ammo.current);

            if (consumesAmmo)
            {
                int reserve = totalAmmo - ammoInMagazine;
                return $"{ammoInMagazine}/{reserve}";
            }
            else
            {
                return $"{ammoInMagazine}";
            }
        }
        else if (consumesAmmo)
        {
            return $"{totalAmmo}";
        }
        else
        {
            return infiniteText;
        }
    }

    public static List<T> ExplosionDetect<T>(Vector3 origin, float range, LayerMask hitDetection) where T : Entity
    {
        // Check for entities
        // Get closest points on their hitboxes
        // Check if those points are within range and angle

        List<T> entities = new List<T>();

        Collider[] hit = Physics.OverlapSphere(origin, range, hitDetection);
        foreach (Collider c in hit)
        {
            // Check if the collider is attached to an entity, that isn't already included
            T e = c.GetComponentInParent<T>();
            if (e == null || entities.Contains(e)) continue;

            // Check for blockages between the origin and the hit point
            Vector3 hitLocation = e.bounds.ClosestPoint(origin);
            bool lineOfSightCheck = AIAction.LineOfSight(origin, hitLocation, hitDetection, e.colliders);
            if (lineOfSightCheck == false) continue;

            entities.Add(e);
        }

        return entities;
    }
    public static List<T> MeleeDetectMultiple<T>(Vector3 origin, Vector3 direction, float range, float angle, LayerMask hitDetection) where T : Entity
    {
        List<T> entities = ExplosionDetect<T>(origin, range, hitDetection);
        entities.RemoveAll((e) =>
        {
            Vector3 hitLocation = e.bounds.ClosestPoint(origin);
            float a = Vector3.Angle(direction, hitLocation - origin);
            return a > angle;
        });
        /*
        MiscFunctions.SortListWithOnePredicate(entities, (e) =>
        {
            Vector3 hitLocation = e.bounds.ClosestPoint(origin);
            return Vector3.Angle(direction, hitLocation - origin);
        });
        */

        return entities;
    }
    /*
    public static T MeleeDetectSingle<T>(Vector3 origin, Vector3 direction, float range, float angle, LayerMask hitDetection) where T : Entity
    {
        List<T> initialDetection = ExplosionDetect(origin, range, hitDetection);

        MiscFunctions.SortListWithOnePredicate(initialDetection, (e) =>
        {
            Vector3 hitLocation = e.bounds.ClosestPoint(origin);
            return Vector3.Angle(direction, hitLocation - origin);
        });

        if (initialDetection.Count < 0) return null;

        T e = initialDetection[0];
        Vector3 hitLocation = e.bounds.ClosestPoint(origin);
        float a = Vector3.Angle(direction, hitLocation - origin);
        if (a > angle) return null;

        return e;
    }
    */
}
