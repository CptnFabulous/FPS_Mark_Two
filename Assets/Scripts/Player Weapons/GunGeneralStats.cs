using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunGeneralStats : MonoBehaviour
{
    [Header("Projectile")]
    public Projectile projectilePrefab;
    public int projectileCount = 1;
    public Transform muzzle;

    [Header("Accuracy")]
    public float sway = 0.2f;
    public float shotSpread = 0;
    public float range = 300;

    [Header("Ammunition")]
    public AmmunitionType ammoType;
    public int ammoPerShot = 1;
    public bool ConsumesAmmo
    {
        get
        {
            return ammoType != null && ammoPerShot > 0;
        }
    }

    [Header("Recoil")]
    public float recoilMagnitude = 2;
    public AnimationCurve recoilCurve;
    public float recoilTime = 0.5f;
    static float recoilSwaySpeed = 10; // I'm not going to bother making this an editable value because it'll probably be exactly the same.
    // (I might take the last 3 of these values and make them values in WeaponHandler instead, since these properties most likely won't change from different guns)

    public UnityEvent effectsOnFire;
    public void Shoot(Character user)
    {
        Vector3 origin = user.LookTransform.position;
        Vector3 aimDirection = user.aimDirection;
        Vector3 worldUp = user.LookTransform.up;
        
        effectsOnFire.Invoke();

        for (int i = 0; i < projectileCount; i++)
        {
            // Instantiate a projectile, enable its gameobject and specify its user
            Projectile newProjectile = Instantiate(projectilePrefab);
            newProjectile.spawnedBy = user;
            newProjectile.gameObject.SetActive(true);

            // Calculates a direction for the projectile, given random spread angles
            Vector3 spreadAngles = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * shotSpread;
            Vector3 projectileDirection = Quaternion.LookRotation(aimDirection, worldUp) * Quaternion.Euler(spreadAngles) * Vector3.forward;
            
            // Calculates a point out based on the angle and range, to aim the projectile if the subsequent raycast fails
            Vector3 hitPoint;
            List<RaycastHit> thingsHit = MiscFunctions.RaycastAllWithExceptions(origin, projectileDirection, range, projectilePrefab.detection, user.health.HitboxColliders);
            if (thingsHit.Count > 0)
            {
                thingsHit.Sort((a, b) => a.distance.CompareTo(b.distance)); // Sort entries by distance
                hitPoint = thingsHit[0].point;

                // If for some reason the hitpoint is behind the muzzle, projectile has no distance to move. Activate OnHit immediately and proceed to next projectile
                if (Vector3.Angle(hitPoint - muzzle.position, projectileDirection) > 90)
                {
                    newProjectile.transform.position = hitPoint;
                    newProjectile.transform.LookAt(transform.position + projectileDirection, worldUp);
                    newProjectile.OnHit(thingsHit[0]);
                    continue;
                }
            }
            else
            {
                hitPoint = origin + (projectileDirection * range);
            }

            // Spawn projectile at muzzle and rotate it towards the hit point
            newProjectile.transform.position = muzzle.position;
            newProjectile.transform.LookAt(hitPoint, worldUp);
        }

        if (user is Player player && recoilMagnitude > 0)
        {
            ApplyRecoil(player.movement);
        }
    }

    void ApplyRecoil(MovementController player)
    {
        float time = Time.time * recoilSwaySpeed;
        float x = Mathf.PerlinNoise(time, 0);
        float y = Mathf.PerlinNoise(0, time);
        x = Mathf.Lerp(-1, 1, x);
        //y = Mathf.Lerp(-1, 1, y);
        Vector2 recoilDirection = new Vector2(x, y).normalized;
        recoilDirection *= recoilMagnitude;

        player.StartCoroutine(player.lookControls.recoilController.AddRecoilOverTime(recoilDirection, recoilTime, recoilCurve));
    }
}
