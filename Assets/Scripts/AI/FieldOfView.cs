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

    public T FindObjectInFieldOfView<T>(System.Predicate<T> criteria, out RaycastHit hit) where T : Entity
    {
        foreach (T e in FindObjectsOfType<T>())
        {
            // Ignore if not meeting the criteria
            if (criteria != null && criteria.Invoke(e) == false) continue;
            // If it is a relevant object, check if the AI can actually see it.
            if (VisionConeCheck(e, out hit) != ViewStatus.Visible) continue;
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
            if (criteria != null && criteria.Invoke(e) == false) return true;
            if (VisionConeCheck(e, out _) != ViewStatus.Visible) return true;
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
    /// <summary>
    /// Calculates and runs a sweep of raycasts to check a cone-shaped area for colliders. This check will also detect partially-hidden colliders, but is performance-intensive and should not be run regularly.
    /// </summary>
    public ViewStatus VisionConeCheck(Entity targetEntity, out RaycastHit hit)
    {
        IList<Collider> targetColliders = targetEntity.colliders;
        if (targetColliders == null || targetColliders.Count <= 0)
        {
            // No colliders are present, do a simple bounds check
            hit = new RaycastHit();
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
            Ray ray = new Ray(transform.position, raycastHitDestination - transform.position);

            // Check if this cast is inside the AI's peripheral vision
            if (AngleCheck(ray.direction, transform, viewingAngles) == false)
            {
                //outOfViewAngle++;
                continue;
            }
            
            // Check that the raycast hits something within range
            if (Physics.Raycast(ray, out hit, viewRange, viewDetection) == false) continue;

            // Check that the hit object was part of the target
            // If the collider matches one in the array, it was
            if (MiscFunctions.ArrayContains(targetColliders, hit.collider) == false)
            {
                blocked++;
                continue;
            }

            // A raycast hit a collider that's part of the target, that means the AI can see it!
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
