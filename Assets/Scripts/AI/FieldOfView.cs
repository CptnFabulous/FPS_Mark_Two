using CptnFabulous.MiscUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ViewStatus
{
    Visible,
    OutOfRange,
    OutsideViewAngle,
    BehindCover,
    NotPresent,
    Blinded,
}

public class FieldOfView : MonoBehaviour
{
    public float viewRange = 25;
    public float peripheralMultiplier = 0.5f;
    public Vector2 viewingAngles = new Vector2(90, 30);
    public DetectionProfile viewDetection;
    public float raycastSpacing = 0.25f;

    AI _root;
    static Vector2Int[] gridPointsToCheck = new Vector2Int[256];
    static int numberOfGridPointsToCheck;

    Collider[] overlapSphereResults = new Collider[1024];
    HashSet<Entity> previouslyScanned = new HashSet<Entity>();
    Entity[] foundTargets = new Entity[256];
    RaycastHit[] foundTargetHits = new RaycastHit[256];
    int numberOfTargets = 0;

    static GenericComparer<Vector2Int> gridSorter = new GenericComparer<Vector2Int>();

    public AI rootAI => _root ??= GetComponentInParent<AI>();

    private void OnDrawGizmosSelected()
    {
        if (MiscFunctions.CurrentCameraNotMain()) return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, viewingAngles.y, viewRange, 0, viewingAngles.x / viewingAngles.y);
    }

    public T FindObjectInFieldOfView<T>(System.Predicate<T> criteria, LayerMask cullingMask, out RaycastHit hit) where T : Entity
    {
        ScanObjectsInFieldOfView(criteria, cullingMask, 1);
        if (numberOfTargets >= 1)
        {
            hit = foundTargetHits[0];
            return foundTargets[0] as T;
        }

        hit = new RaycastHit();
        return null;
    }
    public T[] FindObjectsInFieldOfView<T>(System.Predicate<T> criteria, LayerMask cullingMask) where T : Entity
    {
        ScanObjectsInFieldOfView(criteria, cullingMask, overlapSphereResults.Length);

        T[] entities = new T[numberOfTargets];
        for (int i = 0; i < numberOfTargets; i++)
        {
            entities[i] = foundTargets[i] as T;
        }

        return entities;
    }

    void ScanObjectsInFieldOfView<T>(System.Predicate<T> criteria, LayerMask cullingMask, int maxResults) where T : Entity
    {
        previouslyScanned.Clear();
        numberOfTargets = 0;

        int numberOfResults;
        if (viewingAngles.x <= 180 && viewingAngles.y <= 180)
        {
            // If viewing angle fits comfortably inside a frustum, make a box encompassing it.
            // This reduces the area checked over, and the number of colliders to process.
            float halfRange = viewRange * 0.5f;
            Vector3 boxCentre = transform.position + halfRange * transform.forward;

            // Make box extents
            Vector3 rightmostPoint = Quaternion.Euler(0, viewingAngles.x, 0) * (viewRange * Vector3.forward);
            Vector3 downMostPoint = Quaternion.Euler(viewingAngles.y, 0, 0) * (viewRange * Vector3.forward);
            Vector3 halfExtents = new Vector3(Mathf.Abs(rightmostPoint.x), Mathf.Abs(downMostPoint.y), halfRange);

            // Boxcast
            numberOfResults = Physics.OverlapBoxNonAlloc(boxCentre, halfExtents, overlapSphereResults, transform.rotation, cullingMask);
        }
        else
        {
            // For extra wide FOVs, just use an entire sphere
            numberOfResults = Physics.OverlapSphereNonAlloc(transform.position, viewRange, overlapSphereResults, cullingMask);
        }
        //int numberOfResults = Physics.OverlapSphereNonAlloc(transform.position, viewRange, overlapSphereResults, cullingMask);

        //rootAI.DebugLog($"Checking for objects in field of view. Number of results = {numberOfResults}");
        for (int i = 0; i < numberOfResults; i++)
        {
            //rootAI.DebugLog($"Checking {overlapSphereResults[i]}");

            // Check that an entity exists
            T entity = EntityCache<T>.GetEntity(overlapSphereResults[i].gameObject);
            if (entity == null)
            {
                //rootAI.DebugLog("No entity detected, aborting");
                continue;
            }

            // Don't check this entity again if it's already been scanned
            if (previouslyScanned.Contains(entity))
            {
                //rootAI.DebugLog($"Already scanned {entity}, aborting");
                continue;
            }
            previouslyScanned.Add(entity);

            // Check that it meets the criteria
            if (criteria != null && criteria.Invoke(entity) == false)
            {
                //rootAI.DebugLog($"Criteria not met by {entity}, aborting");
                continue;
            }

            // Do a more complex visual check
            ViewStatus status = VisionConeCheck(entity, out RaycastHit rh);
            if (status != ViewStatus.Visible)
            {
                //.DebugLog($"View status = {status}, aborting");
                continue;
            }

            //rootAI.DebugLog($"Target {entity} is valid, adding");
            foundTargets[numberOfTargets] = entity;
            foundTargetHits[numberOfTargets] = rh;
            numberOfTargets++;

            // Don't continue if we've obtained the necessary number of results
            if (numberOfTargets >= maxResults) return;
        }
    }

    public ViewStatus VisionConeCheck(Vector3 point, out RaycastHit hit) => PositionCheck(point, out hit, null);
    /// <summary>
    /// Calculates and runs a sweep of raycasts to check a cone-shaped area for colliders. This check will also detect partially-hidden colliders, but is performance-intensive and should not be run regularly.
    /// </summary>
    public ViewStatus VisionConeCheck(Entity targetEntity, out RaycastHit hit)
    {
        //Debug.Log($"{rootAI}: vision cone check for {targetEntity}");
        hit = new RaycastHit();

        IList<Collider> targetColliders = targetEntity.colliders;
        // If no colliders are present, just do a simple bounds check
        if (targetColliders == null || targetColliders.Count <= 0) return PositionCheck(targetEntity.CentreOfMass, out _, null);

        Bounds b = targetEntity.bounds;
        Vector3 centre = b.center;

        // Perform a single initial raycast. If this one hits, it means the target is in plain sight and we won't have to bother with any of the partial cover nonsense.
        ViewStatus initialCheckStatus = PositionCheck(centre, out hit, targetColliders);
        if (initialCheckStatus == ViewStatus.Visible) return ViewStatus.Visible;

        #region Calculate grid to check

        // Get centre and axes of raycast sweep zone
        Vector3 directionToTarget = centre - transform.position;
        Quaternion fromOriginToTargetRotation = Quaternion.LookRotation(directionToTarget);
        Vector3 targetRight = fromOriginToTargetRotation * Vector3.right;
        Vector3 targetUp = fromOriginToTargetRotation * Vector3.up;

        // Calculate dimensions of sweep zone, relative to the view direction
        int horizontalAxisIndex = TransformUtility.ClosestAxis(targetRight);
        int verticalAxisIndex = TransformUtility.ClosestAxis(targetUp);
        float zoneWidth = b.size[horizontalAxisIndex];
        float zoneHeight = b.size[verticalAxisIndex];

        // Round up to get the grid dimensions (and number of raycasts)
        Vector2Int gridSize = new Vector2Int();
        gridSize.x = Mathf.CeilToInt(zoneWidth / raycastSpacing);
        gridSize.y = Mathf.CeilToInt(zoneHeight / raycastSpacing);

        // Calculate the spacing between each ray
        float spacingX = zoneWidth / gridSize.x;
        float spacingY = zoneHeight / gridSize.y;
        // Calculate grid start
        Vector3 gridStartCorner = centre + (-targetRight * zoneWidth / 2) + (-targetUp * zoneHeight / 2);

        #endregion

        #region Calculate grid points to check

        numberOfGridPointsToCheck = 0; // Reset number of points
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                gridPointsToCheck[numberOfGridPointsToCheck].x = x;
                gridPointsToCheck[numberOfGridPointsToCheck].y = y;
                //gridPointsToCheck[numberOfGridPointsToCheck] = new Vector2Int(x, y);
                numberOfGridPointsToCheck++;
            }
        }

        // Sort array based on distance from centre
        Vector2 gridCentre = gridSize / 2;
        gridSorter.obtainValue = (v2i) => (v2i - gridCentre).sqrMagnitude;
        Array.Sort(gridPointsToCheck, 0, numberOfGridPointsToCheck, gridSorter);

        #endregion

        #region Check each point for the target

        // Set up values to check why each cast failed
        int outOfViewAngle = 0;
        int blocked = 0;
        for (int i = 0; i < numberOfGridPointsToCheck; i++)
        {
            // Calculates raycast point to aim for
            Vector2 point = gridPointsToCheck[i];
            Vector3 raycastHitDestination = gridStartCorner + (spacingX * (point.x + 0.5f) * targetRight) + (spacingY * (point.y + 0.5f) * targetUp);

            // Perform a line of sight + angle check to the target point, to the target colliders
            ViewStatus castResult = PositionCheck(raycastHitDestination, out hit, targetColliders);
            switch (castResult)
            {
                // If the raycast hit one of the target's colliders, that means the AI can see it!
                case ViewStatus.Visible:
                    return ViewStatus.Visible;
                    break;

                // If outside view angle, ignore and continue.
                case ViewStatus.OutsideViewAngle:
                    outOfViewAngle++;
                    break;

                // If it did hit something else, register as an obstruction
                case ViewStatus.BehindCover:
                    blocked++;
                    break;
            }
        }
        #endregion

        // If the only casts that hit were blocked, the object is behind cover.
        if (blocked > 0) return ViewStatus.BehindCover;

        // If nothing was blocked, but one or more casts was outside the angle, that must be the reason it couldn't be seen.
        if (outOfViewAngle > 0) return ViewStatus.OutsideViewAngle;

        // Otherwise it's probably out of range.
        return ViewStatus.OutOfRange;
    }


    ViewStatus PositionCheck(Vector3 point, out RaycastHit hit, IEnumerable<Collider> targetColliders)
    {
        Vector3 direction = point - transform.position;
        hit = new RaycastHit();

        // Check that the point is not outside of the view angle
        if (AngleCheck(direction, transform, viewingAngles, out float angleX, out _) == false)
        {
            //Debug.Log("Outside view angle");
            return ViewStatus.OutsideViewAngle;
        }

        float calculatedRange = ViewRangeAtAngle(angleX);

        // If we're only checking for a single point:
        if (targetColliders == null || targetColliders.Count() <= 0)
        {
            // Perform a simple range check
            if (direction.magnitude > calculatedRange) return ViewStatus.OutOfRange;
            // Perform a simple line of sight check. If we hit something, line of sight is blocked
            bool seen = AIAction.LineOfSight(transform.position, point, viewDetection.mask, rootAI.HitOwnCollider);
            //Debug.Log($"Simple check, line of sight = {seen}");
            return seen ? ViewStatus.Visible : ViewStatus.BehindCover;
        }

        // TO DO: have some kind of backup check in case all the colliders are disabled for some reason
        //return ViewStatus.NotPresent;

        bool lineOfSight = AIAction.LineOfSightToTarget(transform.position, direction, out hit, calculatedRange, viewDetection.mask, targetColliders, rootAI.HitOwnCollider);
        // If we hit one of the target colliders, then the target is visible!
        if (lineOfSight)
        {
            //Debug.Log("Success!");
            return ViewStatus.Visible;
        }

        // If something was hit that isn't the target, that means it's behind cover.
        if (hit.collider != null)
        {
            //Debug.Log($"Result = {false}, behind cover");
            return ViewStatus.BehindCover;
        }

        // TO DO: should I bother checking if the colliders were in range?
        // The only reason it'd go through with the line of sight to colliders check, not detect anything at all and not be out of range, is if all the colliders were disabled.
        // Or maybe if all the colliders were at the wrong angle compared to the centre of mass?
        // Then again the only time where the collider field is being used is in the main complex check, where the angle is never too different. Otherwise it's private.
        //Debug.Log("Out of range");
        return ViewStatus.OutOfRange;

        // TO DO: check if any of the colliders were even close enough
        //throw new System.NotImplementedException();

    }

    float ViewRangeAtAngle(float angleX) => viewRange;// * Mathf.Lerp(1, peripheralMultiplier, angleX / viewingAngles.x);
    public static bool AngleCheck(Vector3 direction, Transform origin, Vector2 angles, out float x, out float y)
    {
        x = AngleOnAxis(direction, origin.forward, origin.up);
        y = AngleOnAxis(direction, origin.forward, origin.right);
        return x < angles.x && y < angles.y;
    }
    public static float AngleOnAxis(Vector3 direction, Vector3 forward, Vector3 up)
    {
        Vector3 onPlane = Vector3.ProjectOnPlane(direction, up);
        return Vector3.Angle(onPlane, forward);
    }
}