using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAim : MonoBehaviour
{
    public AI ai;
    public Transform viewAxis;
    public AimValues defaultAimStats;

    public void ResetStatsToDefault()
    {
        Stats = defaultAimStats;
    }


    [HideInInspector] public AimValues Stats;
    [HideInInspector] public bool lookingInDefaultDirection = true;
    
    void Awake()
    {
        Stats = defaultAimStats;
        ai.agent.updateRotation = false;
    }
    private void Update()
    {
        if (lookingInDefaultDirection)
        {
            LookInNeutralDirection();
        }
        
        // Obtains a Vector3 value from lookRotation, 'flattened' to perpendicular to the agent's up axis
        Vector3 transformDirection = Vector3.ProjectOnPlane(LookDirection, ai.agent.transform.up);
        // Rotates agent body to match quaternion
        ai.agent.transform.rotation = Quaternion.LookRotation(transformDirection, ai.agent.transform.up);
        // Rotates head to look in the appropriate direction
        viewAxis.rotation = lookRotation;
    }

    #region Look direction values
    Quaternion lookRotation;
    /// <summary>
    /// The point in space the AI looks and aims from.
    /// </summary>
    public Vector3 LookOrigin
    {
        get
        {
            return viewAxis.position;
        }
    }
    /// <summary>
    /// The direction the AI is deliberately aiming towards, excluding sway.
    /// </summary>
    public Vector3 LookDirection
    {
        get
        {
            return lookRotation * Vector3.forward;
        }
    }
    /// <summary>
    /// The direction the AI is looking in, converted into an easy Vector3 value.
    /// </summary>
    public Vector3 AimDirection
    {
        get
        {
            return (lookRotation * AimSway(Stats.swayAngle, Stats.swaySpeed)) * Vector3.forward;
        }
    }
    /// <summary>
    /// A direction directly up perpendicular to the direction the AI is looking.
    /// </summary>
    public Vector3 LookUp
    {
        get
        {
            return lookRotation * Vector3.up;
        }
    }
    /// <summary>
    /// A direction directly right perpendicular to the direction the AI is looking.
    /// </summary>
    public Vector3 LookRight
    {
        get
        {
            return lookRotation * Vector3.right;
        }
    }
    #endregion

    #region Look functions
    public void RotateLookTowards(Vector3 position)
    {
        float degreesPerSecond = Stats.SpeedBasedOnAngle(LookDirection, position - LookOrigin);
        RotateLookTowards(position, degreesPerSecond);
    }
    /// <summary>
    /// Continuously rotates AI aim over time to look at position value, at a speed of degreesPerSecond
    /// </summary>
    /// <param name="position"></param>
    /// <param name="degreesPerSecond"></param>
    public void RotateLookTowards(Vector3 position, float degreesPerSecond)
    {
        Quaternion correctRotation = Quaternion.LookRotation(position - LookOrigin, ai.transform.up);
        //correctRotation *= Quaternion.Inverse(AimSway(Stats.swayAngle, Stats.swaySpeed));
        lookRotation = Quaternion.RotateTowards(lookRotation, correctRotation, degreesPerSecond * Time.deltaTime);
        //Debug.Log(lookRotation.eulerAngles);
    }
    /// <summary>
    /// Continuously rotates AI aim to return to looking in the direction it is moving.
    /// </summary>
    /// <param name="degreesPerSecond"></param>
    public void LookInNeutralDirection()
    {
        NavMeshAgent agent = ai.agent;

        // If agent is moving, look in the direction the agent is moving. Otherwise, look straight forward.
        bool isMoving = ai.agent.velocity.magnitude > 0;
        Vector3 direction = isMoving ? agent.velocity : ai.transform.forward;

        RotateLookTowards(LookOrigin + direction, Stats.lookSpeed);
    }
    #endregion

    #region Look checking
    /// <summary>
    /// Is the AI's look direction at a close enough angle to the desired position?
    /// </summary>
    /// <param name="position"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public bool IsLookingAt(Vector3 target, float angleThreshold, bool useAim = false)
    {
        Vector3 direction = useAim ? AimDirection : LookDirection;
        return Vector3.Angle(target - LookOrigin, direction) <= angleThreshold;
    }

    /// <summary>
    /// Is the position the AI is currently looking at a close enough distance to the desired position?
    /// </summary>
    /// <param name="position"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public bool LookCheckDistance(Vector3 target, float threshold, bool useAim = false)
    {
        Vector3 direction = useAim ? AimDirection : LookDirection;
        Vector3 relativeAimPoint = direction * Vector3.Distance(LookOrigin, target);
        float distanceBetweenAimAndTarget = Vector3.Distance(LookOrigin + relativeAimPoint, target);
        return distanceBetweenAimAndTarget < threshold;
    }

    #endregion

























    /// <summary>
    /// Multiply this by a Quaternion and a Vector3 to get an aim direction with a smooth sway for accuracy deviation.
    /// </summary>
    /// <param name="maxSwayAngle"></param>
    /// <param name="swaySpeed"></param>
    /// <returns></returns>
    public static Quaternion AimSway(float maxSwayAngle, float swaySpeed)
    {
        // Generates changing values from noise
        float noiseX = Mathf.PerlinNoise(Time.time * swaySpeed, 0);
        float noiseY = Mathf.PerlinNoise(0, Time.time * swaySpeed);
        // Converts values from 0 - 1 to -1 - 1
        Vector2 angles = new Vector2(noiseX - 0.5f, noiseY - 0.5f) * 2;
        angles *= maxSwayAngle; //  Multiplies by accuracy value
        // Creates euler angles and combines with current aim direction
        return Quaternion.Euler(angles.y, angles.x, 0);
    }

    [System.Serializable]
    public struct AimValues
    {
        public float lookSpeed;
        public AnimationCurve speedCurve;
        public float swayAngle;
        public float swaySpeed;
        public LayerMask lookDetection;
        public float diameterForUnobstructedSight;
        
        public float SpeedBasedOnAngle(Vector3 currentAimDirection, Vector3 desiredDirection)
        {
            if (speedCurve == null) return lookSpeed;

            float angle = Vector3.Angle(currentAimDirection, desiredDirection);
            return lookSpeed * speedCurve.Evaluate(angle / 180);
        }
    }
}
