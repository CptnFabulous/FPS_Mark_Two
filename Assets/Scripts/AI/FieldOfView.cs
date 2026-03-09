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
    List<Entity> previouslyScanned = new List<Entity>();
    Entity[] foundTargets = new Entity[256];
    RaycastHit[] foundTargetHits = new RaycastHit[256];
    int numberOfTargets = 0;

    static GenericComparer<Vector2Int> gridSorter = new GenericComparer<Vector2Int>();

    public AI rootAI => _root ??= GetComponentInParent<AI>();

    private void OnDrawGizmosSelected()
    {
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

    public ViewStatus VisionConeCheck(Vector3 position)
    {
        Vector3 direction = position - transform.position;

        // Check angle
        if (AngleCheck(direction, transform, viewingAngles, out float angleX, out _) == false) return ViewStatus.OutsideViewAngle;


        // Check range
        if (direction.magnitude > ViewRangeAtAngle(angleX)) return ViewStatus.OutOfRange;
        // Check line of sight
        bool seen = AIAction.LineOfSight(transform.position, position, viewDetection.mask, rootAI.HitOwnCollider);
        return seen ? ViewStatus.Visible : ViewStatus.BehindCover;
    }
    /// <summary>
    /// Calculates and runs a sweep of raycasts to check a cone-shaped area for colliders. This check will also detect partially-hidden colliders, but is performance-intensive and should not be run regularly.
    /// </summary>
    public ViewStatus VisionConeCheck(Entity targetEntity, out RaycastHit hit)
    {
        //Debug.Log($"{rootAI}: vision cone check for {targetEntity}");
        hit = new RaycastHit();

        IList<Collider> targetColliders = targetEntity.colliders;
        // If no colliders are present, just do a simple bounds check
        if (targetColliders == null || targetColliders.Count <= 0) return VisionConeCheck(targetEntity.CentreOfMass);

        #region Calculate grid to check

        Bounds b = targetEntity.bounds;

        // Get centre and axes of raycast sweep zone
        Vector3 centre = b.center;
        Quaternion targetDirection = Quaternion.LookRotation(centre - transform.position);
        Vector3 targetRight = targetDirection * Vector3.right;
        Vector3 targetUp = targetDirection * Vector3.up;

        // Calculate dimensions of sweep zone, relative to the view direction
        int horizontalAxisIndex = MiscFunctions.ClosestAxis(targetRight);
        int verticalAxisIndex = MiscFunctions.ClosestAxis(targetUp);
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
        //int outOfViewAngle = 0;
        int blocked = 0;
        for (int i = 0; i < numberOfGridPointsToCheck; i++)
        {
            Vector2 point = gridPointsToCheck[i];

            // Calculates raycast point to aim for
            //Vector3 raycastHitDestination = gridStartCorner + (spacingX * (point.x) * targetRight) + (spacingY * (point.y) * targetUp);
            Vector3 raycastHitDestination = gridStartCorner + (spacingX * (point.x + 0.5f) * targetRight) + (spacingY * (point.y + 0.5f) * targetUp);
            Vector3 direction = raycastHitDestination - transform.position;

            // Check if this cast is inside the AI's peripheral vision
            if (AngleCheck(direction, transform, viewingAngles, out float angleX, out _) == false)
            {
                //outOfViewAngle++;
                continue;
            }

            float calculatedRange = ViewRangeAtAngle(angleX);

            // Run a line of sight check to the target colliders
            bool lineOfSight = AIAction.LineOfSightToTarget(transform.position, direction, out hit, calculatedRange, viewDetection.mask, targetColliders, QueryTriggerInteraction.Collide, rootAI.HitOwnCollider);
            //Debug.DrawRay(transform.position, viewRange * direction.normalized, lineOfSight ? Color.green : Color.red);
            //Debug.Log(hit.collider);
            // If the raycast hit one of the target's colliders, that means the AI can see it!
            if (lineOfSight) return ViewStatus.Visible;

            // Raycast did not find target.
            // If it did hit something else, register as an obstruction
            if (hit.collider != null) blocked++;
            // Proceed to next check
        }
        #endregion

        // If the only casts that hit were blocked, the object is behind cover.
        // If a cast is outside the viewing angle, the AI couldn't see it anyway, so we didn't bother checking if it's blocked 
        if (blocked > 0) return ViewStatus.BehindCover;

        // Otherwise, the target is completely outside the viewing angle
        return ViewStatus.OutsideViewAngle;
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