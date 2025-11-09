using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewStatus
{
    Visible,
    OutOfRange,
    OutsideViewAngle,
    BehindCover,
    NotPresent,
}

public class FieldOfView : MonoBehaviour
{
    public float viewRange = 25;
    public Vector2 viewingAngles = new Vector2(90, 30);
    public DetectionProfile viewDetection;
    public float raycastSpacing = 0.25f;

    AI _root;
    static List<Vector2Int> gridPointsToCheck = new List<Vector2Int>(); // A cached collection of grid points to check, allowing the checks to fan out in a desired order but without wasting memory on new lists for each call

    static Collider[] overlapSphereResults = new Collider[1024];

    public AI rootAI => _root ??= GetComponentInParent<AI>();

    public T FindObjectInFieldOfView<T>(System.Predicate<T> criteria, LayerMask cullingMask, out RaycastHit hit) where T : Entity
    {
        int numberOfResults = Physics.OverlapSphereNonAlloc(transform.position, viewRange, overlapSphereResults, cullingMask);
        //Debug.Log($"Checking for single object, number = {numberOfResults}");
        for (int i = 0; i < numberOfResults; i++)
        {
            Collider c = overlapSphereResults[i];

            // Check that an entity exists
            T e = EntityCache<T>.GetEntity(c.gameObject);
            if (e == null) continue;

            // Check that it can actually be seen
            if (criteria != null && criteria.Invoke(e) == false) continue;
            if (VisionConeCheck(e, out hit) != ViewStatus.Visible) continue;

            // We've found one!
            return e;
        }

        hit = new RaycastHit();
        return null;
    }
    public List<T> FindObjectsInFieldOfView<T>(System.Predicate<T> criteria, LayerMask cullingMask) where T : Entity
    {
        int numberOfResults = Physics.OverlapSphereNonAlloc(transform.position, viewRange, overlapSphereResults, cullingMask);
        //Debug.Log($"Checking for multiple objects, number = {numberOfResults}");
        List<T> entities = new List<T>();
        for (int i = 0; i < numberOfResults; i++)
        {
            Collider c = overlapSphereResults[i];

            // Check that an entity exists
            T e = EntityCache<T>.GetEntity(c.gameObject);
            if (e == null) continue;

            // Check that it's not already added to the list
            if (entities.Contains(e)) continue;

            // Check that it can actually be seen
            if (criteria != null && criteria.Invoke(e) == false) continue;
            if (VisionConeCheck(e, out _) != ViewStatus.Visible) continue;
            // Entry is detected and should be added to the list
            entities.Add(e);
        }

        return entities;
    }
    public ViewStatus VisionConeCheck(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        // Check range
        if (direction.magnitude > viewRange) return ViewStatus.OutOfRange;
        // Check angle
        if (AngleCheck(direction, transform, viewingAngles) == false) return ViewStatus.OutsideViewAngle;
        // Check line of sight
        bool seen = AIAction.LineOfSight(transform.position, position, viewDetection.mask, rootAI.colliders);
        return seen ? ViewStatus.Visible : ViewStatus.BehindCover;
    }
    /// <summary>
    /// Calculates and runs a sweep of raycasts to check a cone-shaped area for colliders. This check will also detect partially-hidden colliders, but is performance-intensive and should not be run regularly.
    /// </summary>
    public ViewStatus VisionConeCheck(Entity targetEntity, out RaycastHit hit)
    {
        hit = new RaycastHit();

        IList<Collider> targetColliders = targetEntity.colliders;
        if (targetColliders == null || targetColliders.Count <= 0)
        {
            // No colliders are present, do a simple bounds check
            return VisionConeCheck(targetEntity.CentreOfMass);
        }

        #region Calculate grid to check
        Bounds b = targetEntity.bounds;
        // Calculate the sizes of the zone to calculate in 
        float maxExtent = MiscFunctions.Max(b.extents.x, b.extents.y, b.extents.z);
        // Width and height are the same presently but I might make it more precise in the future
        float zoneWidth = maxExtent * 2;
        float zoneHeight = maxExtent * 2;
        // Calculate the dimensions of the grid to raycast in
        Vector2Int grid = new Vector2Int();
        grid.x = Mathf.CeilToInt(zoneWidth / raycastSpacing);
        grid.y = Mathf.CeilToInt(zoneHeight / raycastSpacing);
        // Calculate the spacing between each ray
        float spacingX = zoneWidth / grid.x;
        float spacingY = zoneHeight / grid.y;
        // Calculate grid start
        Vector3 centre = b.center;
        Quaternion targetDirection = Quaternion.LookRotation(centre - transform.position);
        Vector3 targetUp = targetDirection * Vector3.up;
        Vector3 targetRight = targetDirection * Vector3.right;
        Vector3 gridStartCorner = centre + (-targetRight * zoneWidth / 2) + (-targetUp * zoneHeight / 2);
        #endregion

        #region Calculate order of points
        gridPointsToCheck.Clear();
        for (int x = 0; x < grid.x; x++)
        {
            for (int y = 0; y < grid.y; y++)
            {
                gridPointsToCheck.Add(new Vector2Int(x, y));
            }
        }
        Vector2Int gridCentre = grid / 2;
        gridPointsToCheck.Sort((a, b) =>
        {
            // Square magnitudes are quicker for comparison than the regular magnitude
            float aDis = (a - gridCentre).sqrMagnitude;
            float bDis = (b - gridCentre).sqrMagnitude;
            return aDis.CompareTo(bDis);
        });
        #endregion

        #region Check each point for the target

        // Set up values to check why each cast failed
        //int outOfViewAngle = 0;
        int blocked = 0;
        foreach (Vector2Int point in gridPointsToCheck)
        {
            // Calculates raycast point to aim for
            Vector3 raycastHitDestination = gridStartCorner + (targetRight * spacingX * point.x) + (targetUp * spacingY * point.y);
            Vector3 direction = raycastHitDestination - transform.position;
            Ray ray = new Ray(transform.position, direction);

            // Check if this cast is inside the AI's peripheral vision
            if (AngleCheck(ray.direction, transform, viewingAngles) == false)
            {
                //outOfViewAngle++;
                continue;
            }

            // Run a line of sight check to the target colliders
            float distance = Mathf.Min(direction.magnitude, viewRange);
            bool lineOfSight = AIAction.LineOfSightToTarget(ray, out hit, distance, viewDetection.mask, targetColliders, QueryTriggerInteraction.Collide, rootAI.colliders);
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
    public static bool AngleCheck(Vector3 direction, Transform origin, Vector2 angles)
    {
        float x = AngleOnAxis(direction, origin.forward, origin.up);
        float y = AngleOnAxis(direction, origin.forward, origin.right);
        return x < angles.x && y < angles.y;
    }
    public static float AngleOnAxis(Vector3 direction, Vector3 forward, Vector3 up)
    {
        Vector3 onPlane = Vector3.ProjectOnPlane(direction, up);
        return Vector3.Angle(onPlane, forward);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, viewingAngles.y, viewRange, 0, viewingAngles.x / viewingAngles.y);
    }
}