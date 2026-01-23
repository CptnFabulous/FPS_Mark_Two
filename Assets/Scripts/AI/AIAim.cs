using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAim : MonoBehaviour, ICharacterLookController
{
    public enum AILookMode
    {
        FreeLook,
        StraightForward,
        SweepSightline
    }

    public AI ai;
    public Transform viewAxis;
    public Transform bodyTransform;

    [Header("Stats")]
    public AIAimStats defaultAimStats;
    public float targetThresholdAngle = 0;
    /*
    [Range(0, 360)] public float horizontalTurnAngle = 360;
    [Range(0, 180)] public float verticalTurnAngle = 180;
    */
    public bool rotateTorso = true;

    public AIAimStats currentAimStats { get; set; }

    /// <summary>
    /// The world-space position the AI is looking towards. If shifting, represents reference position. If rotating, represents the desired target.
    /// </summary>
    Vector3 lookingTowards;
    AILookMode currentLookMode = AILookMode.FreeLook;

    // Sweep variables
    System.Func<Vector3> getDirectionForSweep;
    Vector2 sweepAngles;
    float delayBetweenSweeps = 0.5f;
    Vector3[] lookTargetAngles;

    #region Properties

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
            /*
            Transform mount = ai.transform;
            if (horizontalTurnAngle < 360 || verticalTurnAngle < 360)
            {
                Quaternion relativeRotationToMount = value * Quaternion.Inverse(mount.rotation);
                Vector3 mountEulerAngles = relativeRotationToMount.eulerAngles;
                mountEulerAngles.y = Mathf.Clamp(mountEulerAngles.y, 0, horizontalTurnAngle);
                mountEulerAngles.x = Mathf.Clamp(mountEulerAngles.x, 0, verticalTurnAngle);
                value = Quaternion.Euler(mountEulerAngles);
            }
            */
            
            if (rotateTorso)
            {
                Vector3 lookDirection = value * Vector3.forward;
                // Obtains a Vector3 value from lookRotation, 'flattened' to perpendicular to the agent's up axis
                Vector3 transformDirection = Vector3.ProjectOnPlane(lookDirection, ai.transform.up);
                // Rotates agent body to match quaternion
                bodyTransform.rotation = Quaternion.LookRotation(transformDirection, ai.transform.up);
            }
            
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
    public Vector3 AimDirection => lookRotation * AimSwayHandler.AimSway(currentAimStats.swayAngle, currentAimStats.swaySpeed) * Vector3.forward;
    public Vector3 upAxis => ai.transform.up;
    Vector2 viewAngles => ai.visionCone.viewingAngles;
    /// <summary>
    /// The total horizontal angle the head will travel as it sweeps from left to right.
    /// </summary>
    float horizontalSweepDistance => Mathf.Max(sweepAngles.x - viewAngles.x, 0);
    /// <summary>
    /// The total vertical angle the head will travel as it passes through each sweep.
    /// </summary>
    float verticalSweepDistance => Mathf.Max(sweepAngles.y - viewAngles.y, 0);
    public float minRange => 0;
    public float maxRange => ai.visionCone.viewRange;

    #endregion

    void Awake()
    {
        currentAimStats = defaultAimStats;
        if (ai.agent != null) ai.agent.updateRotation = false;
    }
    private void OnDrawGizmosSelected()
    {
        // Draw vision cone direction and angles
        Gizmos.matrix = viewAxis.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawRay(Vector3.zero, maxRange * Vector3.forward);
        //Gizmos.DrawFrustum(Vector3.zero, viewAngles.y, maxRange, minRange, viewAngles.x / viewAngles.y);

        // Display gizmos for sightline sweeping code
        if (currentLookMode == AILookMode.SweepSightline)
        {
            Matrix4x4 viewAngleMatrix = Matrix4x4.TRS(viewAxis.position, SweepDirectionQuaternion(), Vector3.one);
            Gizmos.matrix = viewAngleMatrix;
            Vector3 headPos = Vector3.zero;
            //Gizmos.matrix = directionTransform.localToWorldMatrix;
            //Vector3 headPos = directionTransform.InverseTransformPoint(head.position);

            // Draw total range covered by sweep
            Gizmos.color = Color.yellow;
            //Gizmos.DrawFrustum(headPos, sweepAngles.y, maxRange, minRange, sweepAngles.x / sweepAngles.y);
            MiscFunctions.DrawAngledGizmoFrustum(headPos, sweepAngles.x, sweepAngles.y, maxRange, minRange);
            // Draw range the AI needs to turn in
            Gizmos.color = Color.blue;
            //Gizmos.DrawFrustum(headPos, verticalSweepDistance, maxRange, minRange, horizontalSweepDistance / verticalSweepDistance);
            MiscFunctions.DrawAngledGizmoFrustum(headPos, horizontalSweepDistance, verticalSweepDistance, maxRange, minRange);
            //Gizmos.DrawFrustum(headPos, verticalSweepDistance, maxRange, minRange, Mathf.Clamp(horizontalSweepDistance / verticalSweepDistance, 0.01f, 1000f));

            // Draw points to shift towards (don't proceed if there aren't any calculated)
            if (enabled && lookTargetAngles != null)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < lookTargetAngles.Length; i++)
                {
                    Vector3 direction = SweepPointEuler(i) * Vector3.forward;
                    Gizmos.DrawLine(headPos + (direction * minRange), headPos + (direction * maxRange));
                }
            }
        }
    }


    #region Look functions

    public void RotateFreeLookTowards(Vector3 position)
    {
        currentLookMode = AILookMode.FreeLook;
        RotateLookTowards(position);
    }
    void RotateLookTowards(Vector3 position)
    {
        float degreesPerSecond = currentAimStats.SpeedBasedOnAngle(LookDirection, position - LookOrigin);
        //ai.DebugLog(degreesPerSecond);
        RotateLookTowards(position, degreesPerSecond);
    }
    public void ShiftLookTowards(Vector3 position, float distancePerSecond)
    {
        currentLookMode = AILookMode.FreeLook;
        lookingTowards = Vector3.MoveTowards(lookingTowards, position, distancePerSecond * Time.deltaTime);
        lookRotation = Quaternion.LookRotation(lookingTowards - LookOrigin, upAxis);
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
        //Debug.Log($"{position}, {lookingTowards}, {LookOrigin}");
        Quaternion correctRotation = Quaternion.LookRotation(position - LookOrigin, upAxis);
        //correctRotation *= Quaternion.Inverse(AimSway(Stats.swayAngle, Stats.swaySpeed));
        lookRotation = Quaternion.RotateTowards(lookRotation, correctRotation, degreesPerSecond * Time.deltaTime);
    }
    public void ResetStatsToDefault() => currentAimStats = defaultAimStats;

    /// <summary>
    /// Continuously rotates AI aim to return to looking in the direction it is moving.
    /// </summary>
    /// <param name="degreesPerSecond"></param>
    public void LookInNeutralDirection()
    {
        // If already running a neutral direction look coroutine, do nothing
        if (currentLookMode == AILookMode.StraightForward) return;
        StartCoroutine(LookInNeutralDirectionAsync());
        /*
        NavMeshAgent agent = ai.agent;

        // If agent is moving, look in the direction the agent is moving. Otherwise, look straight forward.
        bool isMoving = ai.agent.velocity.magnitude > 0;
        Vector3 direction = isMoving ? agent.velocity : ai.transform.forward;

        RotateLookTowards(LookOrigin + direction, Stats.lookSpeed);
        */
    }
    public void SweepSightline(System.Func<Vector3> obtainDirection, Vector2 angles, float delayBetweenSweeps)
    {
        // If already set to sweep a sightline, do nothing
        if (currentLookMode == AILookMode.SweepSightline) return;
        StartCoroutine(SweepSightlineAsync(obtainDirection, angles, delayBetweenSweeps));
    }
    public void CancelAsyncRoutines()
    {
        ai.DebugLog("Cancelling async look routines");
        currentLookMode = AILookMode.FreeLook;
    }
    

    #endregion

    #region Async functions

    public IEnumerator RotateTowardsAsync(Vector3 position)
    {
        //ai.DebugLog($"Rotating look towards {position}");
        //Debug.Log(enabled);
        //Debug.Log(IsLookingAt(position));
        while (enabled && !IsLookingAt(position))
        {
            yield return null;
            RotateLookTowards(position);

            // TO DO: if AI is unable to rotate any further towards the target, yield break
        }
    }
    public IEnumerator LookInNeutralDirectionAsync()
    {
        while (enabled && currentLookMode == AILookMode.StraightForward)
        {
            yield return null;

            // If agent is moving, look in the direction the agent is moving. Otherwise, look straight forward.
            NavMeshAgent agent = ai.agent;
            bool isMoving = agent.velocity.magnitude > 0;
            Vector3 direction = isMoving ? agent.velocity : ai.transform.forward;
            RotateLookTowards(LookOrigin + direction, currentAimStats.lookSpeed);
        }
    }
    public IEnumerator SweepSightlineAsync(System.Func<Vector3> obtainDirection, Vector2 angles, float delayBetweenSweeps)
    {
        // Declare this component is currently doing a sightline sweep
        currentLookMode = AILookMode.SweepSightline;

        #region Set stats

        getDirectionForSweep = obtainDirection;
        this.sweepAngles.x = Mathf.Clamp(angles.x, 0, 360);
        this.sweepAngles.y = Mathf.Clamp(angles.y, 0, 180);
        this.delayBetweenSweeps = delayBetweenSweeps;

        #endregion

        #region Set up the necessary positions to sweep between

        // Divide vertical sweep angle by vertical FOV to determine number of horizontal sweeps
        int numberOfSweeps = Mathf.CeilToInt(sweepAngles.y / viewAngles.y);


        // Calculate halves of the total horizontal angles
        float halfOfHorizontalAngle = (horizontalSweepDistance > 0) ? (horizontalSweepDistance / 2) : 0;
        float halfOfVerticalAngle = (verticalSweepDistance > 0) ? (verticalSweepDistance / 2) : 0;
        // Divide total vertical travel by the number of spaces between each sweep point (number of sweeps - 1).
        float toTravelBetweenSweeps = (numberOfSweeps > 1) ? verticalSweepDistance / (numberOfSweeps - 1) : 0;
        // For these 3 values, include corrections for dividing by zero so we don't cause NaN values with the quaternions

        //Debug.Log($"{halfOfHorizontalAngle}, {halfOfVerticalAngle}, {toTravelBetweenSweeps}");

        // Generate array of points for the camera to move between
        lookTargetAngles = new Vector3[numberOfSweeps * 2];
        for (int i = 0; i < numberOfSweeps; i++)
        {
            // Figure out the angle height for the two positions in the sweep
            float verticalSweepAngle = toTravelBetweenSweeps * i;

            // Offset from '0 to 1' to '-0.5 to 0.5'
            verticalSweepAngle -= halfOfVerticalAngle;

            // Calculate the two points to angle between for this sweep (current vertical, leftmost for one and rightmost for the other)
            int indexOfLeft = i * 2;
            lookTargetAngles[indexOfLeft] = new Vector3(verticalSweepAngle, -halfOfHorizontalAngle, 0); // Left
            lookTargetAngles[indexOfLeft + 1] = new Vector3(verticalSweepAngle, halfOfHorizontalAngle, 0); // Right
        }

        #endregion

        // TO DO: If there aren't enough points to justify rotating between, just stare in the target direction.
        if (lookTargetAngles.Length < 2)
        {
            Debug.LogError($"{this}: stare function not implemented!");
            /*
            Vector3 direction = getDirectionForSweep.Invoke() * Vector3.forward;
            yield return RotateTowardsAsync(direction);
            */
            yield break;
        }

        // Shift look between different positions
        while (enabled && currentLookMode == AILookMode.SweepSightline)
        {
            // Lerp look between points
            for (int i = 0; i < lookTargetAngles.Length; i++)
            {

                Quaternion sweepPointOffsetQuaternion = SweepPointEuler(i);
                Quaternion desiredQuaternion = sweepPointOffsetQuaternion * SweepDirectionQuaternion();
                Vector3 desiredDirection = desiredQuaternion * Vector3.forward;


                Vector3 toLookTowards = viewAxis.position + desiredDirection;

                //Debug.Log($"{ai} SweepSightlineAsync() point {i + 1}/{lookTargetAngles.Length}, frame {Time.frameCount}");
                //Debug.Log($"{ai} SweepSightlineAsync() point {i + 1}/{lookTargetAngles.Length}, frame {Time.frameCount}. {viewAxis.position + viewAxis.forward}, {toLookTowards}");
                //Debug.Log($"Euler angles = {lookTargetAngles[i]}. Quaternions = {sweepPointOffsetQuaternion}, {desiredQuaternion}");
                //Debug.Log($"{desiredDirection}, {toLookTowards}.");

                yield return RotateTowardsAsync(toLookTowards);

                // Wait for the desired delay before looking towards the next target.
                yield return new WaitForSeconds(delayBetweenSweeps);
            }
        }
    }

    Quaternion SweepDirectionQuaternion()
    {
        Vector3 direction = getDirectionForSweep.Invoke();
        //Debug.Log($"{direction}, {upAxis}");
        return Quaternion.LookRotation(direction, upAxis);
    }
    Quaternion SweepPointEuler(int index)
    {
        //Debug.Log(lookTargetAngles[index]);
        return Quaternion.Euler(lookTargetAngles[index]);
    }

    #endregion

    #region Obtaining data
    /// <summary>
    /// Is the AI's look direction at a close enough angle to the desired position?
    /// </summary>
    /// <param name="position"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public bool IsLookingAt(Vector3 target, bool useAim = false)
    {
        
        Vector3 direction = useAim ? AimDirection : LookDirection;
        bool value = Vector3.Angle(target - LookOrigin, direction) <= targetThresholdAngle;
        //ai.DebugLog($"Is looking at {target} = {value}");
        return value;
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
    /*
    [System.Serializable]
    public class AimValues
    {
        public float lookSpeed = 360;
        public AnimationCurve speedCurve = AnimationCurve.Linear(0, 1, 1, 1);
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
    */
}
