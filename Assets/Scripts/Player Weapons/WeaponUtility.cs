using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUtility : MonoBehaviour
{
    /// <summary>
    /// Multiply this by a Quaternion and a Vector3 to get an aim direction with a smooth sway for accuracy deviation.
    /// </summary>
    /// <param name="maxSwayAngle"></param>
    /// <param name="swaySpeed"></param>
    /// <returns></returns>
    public static Quaternion AimSway(float maxSwayAngle, float swaySpeed)
    {
        // Generates changing values from noise
        float t = Time.time * swaySpeed;
        float noiseX = Mathf.PerlinNoise(t, 0);
        float noiseY = Mathf.PerlinNoise(0, t);
        // Converts values from 0 - 1 to -1 - 1
        Vector2 angles = new Vector2(noiseX - 0.5f, noiseY - 0.5f) * 2;
        angles *= maxSwayAngle; //  Multiplies by accuracy value
        // Creates euler angles and combines with current aim direction
        return Quaternion.Euler(angles.y, angles.x, 0);
    }
    public static void CalculateObjectLaunch(Vector3 aimOrigin, Vector3 launchOrigin, Vector3 direction, float range, LayerMask detection, IList<Collider> exceptions, out Vector3 launchDirection, out Vector3 castHitPoint, out RaycastHit rh, out bool hitPointIsBehindMuzzle)
    {
        Debug.DrawRay(aimOrigin, direction * range, Color.magenta, 5);

        bool successfulCast = MiscFunctions.RaycastWithExceptions(aimOrigin, direction, out rh, range, detection, exceptions);
        // Calculate where the projectile needs to go
        castHitPoint = successfulCast ? rh.point : aimOrigin + (direction * range);
        // Calculate the direction the projectile needs to move in
        launchDirection = castHitPoint - launchOrigin;
        // Check the dot product in case the hit point is behind the muzzle (e.g. if the player is shooting into a wall)
        hitPointIsBehindMuzzle = successfulCast && Vector3.Dot(launchDirection, direction) < 0;

        Debug.DrawLine(launchOrigin, castHitPoint, Color.blue, 5);
        //Debug.DrawRay(launchOrigin, launchDirection * range, Color.blue, 5);
    }
    public static string AmmoCounterHUDDisplay(RangedAttack rangedAttack, string infiniteText = "")
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
}
