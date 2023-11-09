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
    public LayerMask viewDetection = ~0;
    public float raycastSpacing = 0.25f;

    AI _root;
    static List<Vector2Int> gridPointsToCheck = new List<Vector2Int>(); // A cached collection of grid points to check, allowing the checks to fan out in a desired order but without wasting memory on new lists for each call

    public AI rootAI => _root ??= GetComponentInParent<AI>();

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, viewingAngles.y, viewRange, 0, viewingAngles.x / viewingAngles.y);
    }

    public T FindObjectInFieldOfView<T>(System.Predicate<T> criteria, out RaycastHit hit) where T : Entity
    {
        foreach (T e in FindObjectsOfType<T>())
        {
            // Ignore if not meeting the criteria
            if (criteria.Invoke(e) == false) continue;
            // If it is a relevant object, check if the AI can actually see it.
            if (VisionConeCheck(e.colliders, out hit) != ViewStatus.Visible) continue;
            // We've found one!
            return e;
        }
        hit = new RaycastHit();
        return null;
    }
    public List<T> FindObjectsInFieldOfView<T>(System.Predicate<T> criteria) where T : Entity
    {
        List<T> entities = new List<T>(FindObjectsOfType<T>());
        entities.RemoveAll((e) =>
        {
            if (criteria.Invoke(e) == false) return true;
            if (VisionConeCheck(e.colliders, out _) != ViewStatus.Visible) return true;
            return false;
        });
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
        bool seen = AIAction.LineOfSight(transform.position, position, viewDetection, rootAI.colliders);
        return seen ? ViewStatus.Visible : ViewStatus.BehindCover;
    }
    public ViewStatus VisionConeCheck(IList<Collider> targetColliders, out RaycastHit hit)
    {
        return VisionConeCheck(targetColliders, transform, viewingAngles, viewRange, out hit, viewDetection, raycastSpacing);
    }

    /// <summary>
    /// Calculates and runs a sweep of raycasts to check a cone-shaped area for colliders. This check will also detect partially-hidden colliders, but is performance-intensive and should not be run regularly.
    /// </summary>
    public static ViewStatus VisionConeCheck(IList<Collider> targetColliders, Transform origin, Vector2 angles, float range, out RaycastHit hit, LayerMask viewDetection, float raycastSpacing = 0.25f)
    {
        Bounds b = MiscFunctions.CombinedBounds(targetColliders);

        #region Calculate grid to check
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
        Quaternion targetDirection = Quaternion.LookRotation(centre - origin.position);
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
            Vector3 raycastDirection = raycastHitDestination - origin.position;
            // If raycast hits within range and is inside the valid detection angle

            // Check if this cast is inside the AI's peripheral vision
            if (AngleCheck(raycastDirection, origin, angles) == false)
            //if (Vector3.Angle(origin.forward, raycastDirection) >= angle)
            {
                //outOfViewAngle++;
                continue;
            }
            // Check that something is hit
            if (Physics.Raycast(origin.position, raycastDirection, out hit, range, viewDetection) == false)
            {
                continue;
            }
            // Check that the hit object was part of the target
            if (MiscFunctions.ArrayContains(targetColliders, hit.collider) == false)
            {
                blocked++;
                continue;
            }

            return ViewStatus.Visible;
        }
        #endregion

        hit = new RaycastHit();

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
}
