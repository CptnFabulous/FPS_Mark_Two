using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public Character info;
    public Health healthData;
    public NavMeshAgent agent;

    [Header("Viewing")]
    public Transform viewAxis;
    public float viewRange = 25;
    public Vector2 viewingAngles = new Vector2(60, 30);
    public LayerMask viewDetection = ~0;
    public float hearingRange = 50;
    
    public Collider[] ObjectsInFieldOfView
    {
        get
        {
            List<Collider> objects = new List<Collider>(Physics.OverlapSphere(LookOrigin, viewRange, viewDetection));
            for (int i = 0; i < objects.Count; i++)
            {
                #region Angle check
                float distance = Vector3.Distance(LookOrigin, objects[i].bounds.center);
                Vector3 roughlyAdjacentPosition = LookOrigin + (LookForward * distance);
                Vector3 closestPoint = objects[i].ClosestPoint(roughlyAdjacentPosition);
                // Produces a quaternion pointing from the look origin towards the closest point
                Quaternion closestPointDirectionRotation = Quaternion.LookRotation(closestPoint - LookOrigin, LookUp);
                // Makes that quaternion relative to the direction the AI is looking in
                Quaternion differenceRotation = lookRotation * Quaternion.Inverse(closestPointDirectionRotation);
                // If the angles are within the viewing angles
                if (!(differenceRotation.eulerAngles.x < viewingAngles.y && differenceRotation.eulerAngles.y < viewingAngles.x))
                {
                    // if the closest part of the collider is outside the angles, no part of the collider is inside
                    objects[i] = null;
                    continue;
                }
                #endregion


                #region Line of sight check

                #endregion

            }
            objects.RemoveAll(o => o == null); // Prune list of null entries so only relevant results remain
            return objects.ToArray();
        }
    }

    /// <summary>
    /// Calculates and runs a sweep of raycasts to check an area for sections of a particular collider. This is performance-intensive and should not be run regularly.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="raycastSpacing"></param>
    /// <param name="hit"></param>
    /// <param name="viewDetection"></param>
    /// <returns></returns>
    public static bool ComplexLineOfSightCheck(Vector3 origin, Collider target, float raycastSpacing, out RaycastHit hit, LayerMask viewDetection)
    {
        Bounds b = target.bounds;
        Vector3 centre = b.center;
        float maxExtent = b.extents.magnitude;
        Quaternion lookDirection = Quaternion.LookRotation(centre - origin);
        Vector3 up = lookDirection * Vector3.up;
        Vector3 right = lookDirection * Vector3.right;

        float zoneWidth = Vector3.Distance(centre + (-right * maxExtent), centre + (right * maxExtent));
        int gridWidth = Mathf.CeilToInt(zoneWidth / raycastSpacing);
        float spacingX = zoneWidth / gridWidth;

        float zoneHeight = Vector3.Distance(centre + (-up * maxExtent), centre + (up * maxExtent));
        int gridHeight = Mathf.CeilToInt(zoneHeight / raycastSpacing);
        float spacingY = zoneHeight / gridHeight;
        
        Vector3 gridStartCorner = centre + (-right * zoneWidth / 2) + (-up * zoneHeight / 2);
        float range = Vector3.Distance(origin, centre) + maxExtent;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 raycastHitDestination = gridStartCorner + (right * spacingX * x) + (up * spacingY * y);
                Vector3 direction = raycastHitDestination - origin;
                if (Physics.Raycast(origin, direction, out hit, range, viewDetection))
                {
                    Debug.DrawRay(origin, direction.normalized * range, Color.green, 3);
                    if (hit.collider == target)
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.DrawRay(origin, direction.normalized * range, Color.yellow, 3);
                }
            }
        }

        hit = new RaycastHit();
        return false;
    }

    #region Looking and aiming
    /// <summary>
    /// Represents the current direction the AI is looking in.
    /// </summary>
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
    /// The direction the AI is looking in, converted into an easy Vector3 value.
    /// </summary>
    public Vector3 LookForward
    {
        get
        {
            return lookRotation * Vector3.forward;
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
    /// Continuously rotates AI aim over time to look at position value, at a speed of degreesPerSecond
    /// </summary>
    /// <param name="position"></param>
    /// <param name="degreesPerSecond"></param>
    public void RotateLookTowards(Vector3 position, float degreesPerSecond)
    {
        Quaternion correctRotation = Quaternion.LookRotation(position - LookOrigin, transform.up);
        lookRotation = Quaternion.RotateTowards(lookRotation, correctRotation, degreesPerSecond * Time.deltaTime);
    }

    /// <summary>
    /// Is the AI looking close enough to the position to meet the angle threshold?
    /// </summary>
    /// <param name="position"></param>
    /// <param name="threshold"></param>
    /// <returns></returns>
    public bool IsLookingAt(Vector3 position, float threshold)
    {
        return Vector3.Angle(position - LookOrigin, LookForward) <= threshold;
    }

    /// <summary>
    /// Continuously rotates AI aim to return to looking in the direction it is moving.
    /// </summary>
    /// <param name="degreesPerSecond"></param>
    public void ReturnToNeutralLookPosition(float degreesPerSecond)
    {
        RotateLookTowards(LookOrigin + transform.forward, degreesPerSecond);
    }
    /*
    // Rotates AI aim to look at something, in a specified time.
    public IEnumerator LookAtThing(Vector3 position, float lookTime, AnimationCurve lookCurve)
    {
        inLookIENumerator = true;

        float timer = 0;

        Quaternion originalRotation = lookDirectionQuaternion;

        while (timer < 1)
        {
            timer += Time.deltaTime / lookTime;

            lookDirectionQuaternion = Quaternion.Lerp(originalRotation, Quaternion.LookRotation(position, transform.up), lookCurve.Evaluate(timer));

            yield return null;
        }

        inLookIENumerator = false;
        print("Agent is now looking at " + position + ".");
    }
    */
    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.matrix = viewAxis.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, viewingAngles.y, viewRange, 0, viewingAngles.x / viewingAngles.y);
    }
}
