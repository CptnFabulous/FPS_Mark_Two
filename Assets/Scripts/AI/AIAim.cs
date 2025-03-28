using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAim : MonoBehaviour, ICharacterLookController
{
    public AI ai;
    public Transform viewAxis;
    public AimValues defaultAimStats;
    public float targetThresholdAngle = 0;

    public void ResetStatsToDefault()
    {
        Stats = defaultAimStats;
    }


    [HideInInspector] public AimValues Stats;
    [HideInInspector] public bool lookingInDefaultDirection = true;

    /// <summary>
    /// The world-space position the AI is looking towards. If shifting, represents reference position. If rotating, represents the desired target.
    /// </summary>
    public Vector3 lookingTowards { get; private set; }

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
    }

    #region Look direction values
    public bool active
    {
        get => enabled;
        set => enabled = value;
    }
    public Quaternion lookRotation
    {
        get => viewAxis.rotation;
        set
        {
            Vector3 LookDirection = value * Vector3.forward;
            // Obtains a Vector3 value from lookRotation, 'flattened' to perpendicular to the agent's up axis
            Vector3 transformDirection = Vector3.ProjectOnPlane(LookDirection, ai.agent.transform.up);
            // Rotates agent body to match quaternion
            ai.agent.transform.rotation = Quaternion.LookRotation(transformDirection, ai.agent.transform.up);
            // Rotates head to look in the appropriate direction
            viewAxis.rotation = value;
        }
    }
    /// <summary>
    /// The point in space the AI looks and aims from.
    /// </summary>
    public Vector3 LookOrigin => viewAxis.position;
    /// <summary>
    /// The direction the AI is deliberately aiming towards, excluding sway.
    /// </summary>
    public Vector3 LookDirection => viewAxis.forward;
    /// <summary>
    /// The direction the AI is looking in, converted into an easy Vector3 value.
    /// </summary>
    public Vector3 AimDirection => lookRotation * AimSwayHandler.AimSway(Stats.swayAngle, Stats.swaySpeed) * Vector3.forward;
    #endregion

    #region Look functions
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
    public void RotateLookTowards(Vector3 position)
    {
        lookingInDefaultDirection = false;
        float degreesPerSecond = Stats.SpeedBasedOnAngle(LookDirection, position - LookOrigin);
        RotateLookTowards(position, degreesPerSecond);
    }
    /// <summary>
    /// Rotates AI aim over time to look at position value, at a speed of degreesPerSecond
    /// </summary>
    /// <param name="position"></param>
    /// <param name="degreesPerSecond"></param>
    void RotateLookTowards(Vector3 position, float degreesPerSecond)
    {
        /*
        // An experimental version that should ensure the position doesn't snap when switching between rotate and shift based look functions
        Vector3 direction = Vector3.RotateTowards(lookingTowards - LookOrigin, position - LookOrigin, degreesPerSecond * Mathf.Deg2Rad, 0);
        lookRotation = Quaternion.LookRotation(direction, ai.transform.up);
        lookingTowards = LookOrigin + direction;
        */

        lookingTowards = position;
        Quaternion correctRotation = Quaternion.LookRotation(position - LookOrigin, ai.transform.up);
        //correctRotation *= Quaternion.Inverse(AimSway(Stats.swayAngle, Stats.swaySpeed));
        lookRotation = Quaternion.RotateTowards(lookRotation, correctRotation, degreesPerSecond * Time.deltaTime);
    }
    public void ShiftLookTowards(Vector3 position, float distancePerSecond)
    {
        lookingInDefaultDirection = false;
        lookingTowards = Vector3.MoveTowards(lookingTowards, position, distancePerSecond * Time.deltaTime);
        lookRotation = Quaternion.LookRotation(lookingTowards - LookOrigin, ai.transform.up);
    }
    
    #endregion

    #region Look checking
    /// <summary>
    /// Is the AI's look direction at a close enough angle to the desired position?
    /// </summary>
    /// <param name="position"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public bool IsLookingAt(Vector3 target, bool useAim = false)
    {
        Vector3 direction = useAim ? AimDirection : LookDirection;
        return Vector3.Angle(target - LookOrigin, direction) <= targetThresholdAngle;
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

    public IEnumerator RotateTowards(Vector3 position)
    {
        while (IsLookingAt(position) == false)
        {
            yield return null;
            RotateLookTowards(position);
        }
    }
    
    // UNTESTED: Sweeps all around the AI 
    public IEnumerator SweepSurroundings()
    {
        yield break;

        float distance = 5; // I'm pretty sure this could be anything

        int count = Mathf.CeilToInt(360 / ai.visionCone.viewingAngles.y);
        float angleSegment = 180 / (count - 1);

        Vector3 eulerAngles = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            eulerAngles.y = 0; // Resets horizontal direction
            eulerAngles.x = (angleSegment * i) - 90; // Sets vertical direction (shifts from -90 to 90)

            for (int j = 0; j < 4; j++)
            {
                eulerAngles.y += 90;

                Vector3 p = Quaternion.Euler(eulerAngles) * Vector3.forward;
                p *= distance;

                Debug.Log($"{i} = {eulerAngles.x}, {j} = {eulerAngles.y}");

                yield return RotateTowards(viewAxis.TransformPoint(p));
            }
        }
    }
    /*
    void SweepCalculation(Transform t, int count, float distance, Color debugColour)
    {
        Vector3 previousPoint = Vector3.zero;


        Vector3 eulerAngles = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            // Goes from -90 to 90
            eulerAngles.y = 0;

            eulerAngles.x = 180 / (count - 1) * i;
            eulerAngles.x -= 90;

            for (int j = 0; j < 4; j++)
            {
                eulerAngles.y += 90;

                Vector3 p = Quaternion.Euler(eulerAngles) * Vector3.forward;
                p *= distance;


                Debug.DrawLine(t.TransformPoint(previousPoint), t.TransformPoint(p), debugColour);
                previousPoint = p;
            }
        }
    }
    */





















    

    [System.Serializable]
    public struct AimValues
    {
        public float lookSpeed;
        public AnimationCurve speedCurve;
        public float swayAngle;
        public float swaySpeed;
        //public LayerMask lookDetection;
        public float diameterForUnobstructedSight;
        
        public float SpeedBasedOnAngle(Vector3 currentAimDirection, Vector3 desiredDirection)
        {
            if (speedCurve == null) return lookSpeed;

            float angle = Vector3.Angle(currentAimDirection, desiredDirection);
            return lookSpeed * speedCurve.Evaluate(angle / 180);
        }
    }
}
