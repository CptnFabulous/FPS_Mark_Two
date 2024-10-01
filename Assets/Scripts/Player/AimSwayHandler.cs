using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimSwayHandler : MonoBehaviour
{
    public float baseAccuracyMultiplier = 1;
    public float swaySpeed = 0.5f;
    public MultiplierStack swayMultipliers = new MultiplierStack();
    public float swayAcceleration = 20;

    [Header("ADS stats")]
    public float adsMultiplier = 1;
    public string adsMultiplierReference = "Aiming down sights";

    [Header("References")]
    public Transform aimAxis;
    public WeaponHandler weaponHandler;

    float _currentSway;

    public Weapon currentWeapon => weaponHandler.CurrentWeapon;
    public float aimSwayAngle => _currentSway;
    float desiredSwayAngle
    {
        get
        {
            float totalSway = 0;
            if (currentWeapon != null && currentWeapon.CurrentMode is RangedAttack ra)
            {
                totalSway += ra.stats.sway;
            }

            //Debug.Log(totalSway);
            totalSway *= baseAccuracyMultiplier;
            totalSway *= swayMultipliers.calculatedValue;

            return totalSway;
        }
    }
    /// <summary>
    /// The direction the player is currently aiming in, accounting for accuracy sway.
    /// </summary>
    public Vector3 aimDirection => aimAxis.rotation * AimSway(aimSwayAngle, swaySpeed) * Vector3.forward;

    private void Update()
    {
        // Gradually shifts aim sway towards the desired value
        _currentSway = Mathf.MoveTowards(_currentSway, desiredSwayAngle, swayAcceleration * Time.deltaTime);
    }

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
}