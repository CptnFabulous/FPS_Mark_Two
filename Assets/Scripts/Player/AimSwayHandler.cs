using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimSwayHandler : MonoBehaviour
{
    public float baseAccuracyMultiplier = 1;
    public float swaySpeed = 0.5f;
    public AnimationCurve centeringCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public MultiplierStack swayMultipliers = new MultiplierStack();
    public float swayAcceleration = 20;

    [Header("ADS stats")]
    public float adsMultiplier = 1;
    public string adsMultiplierReference = "Aiming down sights";

    [Header("References")]
    public Transform aimAxis;
    public WeaponHandler weaponHandler;

    float _currentSway;
    Quaternion _currentSwayQuaternion;

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

    public Vector3 aimOrigin => transform.position;
    public Quaternion aimRotation => aimAxis.rotation * _currentSwayQuaternion;//AimSway(aimSwayAngle, swaySpeed, centeringCurve);
    /// <summary>
    /// The direction the player is currently aiming in, accounting for accuracy sway.
    /// </summary>
    public Vector3 aimDirection => aimRotation * Vector3.forward;

    private void Update()
    {
        // Gradually shifts aim sway towards the desired value
        _currentSway = Mathf.MoveTowards(_currentSway, desiredSwayAngle, swayAcceleration * Time.deltaTime);
        _currentSwayQuaternion = AimSway(aimSwayAngle, swaySpeed, centeringCurve);
    }
    private void OnDrawGizmosSelected()
    {
        float length = 20;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(aimAxis.position, length * aimDirection);

        Gizmos.matrix = aimAxis.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Gizmos.DrawFrustum(Vector3.zero, aimSwayAngle, length, 0, 1);
    }

    /// <summary>
    /// Multiply this by a Quaternion and a Vector3 to get an aim direction with a smooth sway for accuracy deviation.
    /// </summary>
    /// <param name="maxSwayAngle"></param>
    /// <param name="swaySpeed"></param>
    /// <returns></returns>
    public static Quaternion AimSway(float maxSwayAngle, float swaySpeed, AnimationCurve centeringCurve = null)
    {
        // Generates changing values from noise
        float t = Time.time * swaySpeed;
        float noiseX = Mathf.PerlinNoise(t, 0);
        float noiseY = Mathf.PerlinNoise(0, t);
        // Converts values from 0 - 1 to -1 - 1
        Vector2 angles = new Vector2(noiseX - 0.5f, noiseY - 0.5f) * 2;

        // Evaluate the magnitude based on a curve (to bias shots towards or away from the centre)
        if (centeringCurve != null)
        {
            angles = angles.normalized * centeringCurve.Evaluate(angles.magnitude);
        }

        // Multiply by accuracy value and create euler angles
        angles *= maxSwayAngle;
        return Quaternion.Euler(angles.y, angles.x, 0);
    }
}