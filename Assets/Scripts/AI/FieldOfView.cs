using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRange = 25;
    public Vector2 viewingAngles = new Vector2(60, 30);
    public LayerMask viewDetection = ~0;
    public float raycastSpacing = 0.25f;

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, viewingAngles.y, viewRange, 0, viewingAngles.x / viewingAngles.y);
    }
    public bool VisionConeCheck(IList<Collider> targetColliders, out RaycastHit hit)
    {
        return VisionConeCheck(targetColliders, transform, viewingAngles, viewRange, out hit, viewDetection, raycastSpacing);
    }
    
    /// <summary>
    /// Calculates and runs a sweep of raycasts to check a cone-shaped area for colliders. This check will also detect partially-hidden colliders, but is performance-intensive and should not be run regularly.
    /// </summary>
    public static bool VisionConeCheck(IList<Collider> targetColliders, Transform origin, Vector2 angles, float range, out RaycastHit hit, LayerMask viewDetection, float raycastSpacing = 0.25f)
    {
        Bounds b = MiscFunctions.CombinedBounds(targetColliders);

        // Calculate the sizes of the zone to calculate in (width and height are the same presently but I might make it more precise)
        float maxExtent = MiscFunctions.Max(b.extents.x, b.extents.y, b.extents.z);
        float zoneWidth = maxExtent * 2;
        float zoneHeight = maxExtent * 2;
        // Calculate the dimensions of the grid to raycast in
        int gridWidth = Mathf.CeilToInt(zoneWidth / raycastSpacing);
        int gridHeight = Mathf.CeilToInt(zoneHeight / raycastSpacing);
        // Calculate the spacing between each ray
        float spacingX = zoneWidth / gridWidth;
        float spacingY = zoneHeight / gridHeight;
        // Calculate grid start
        Vector3 centre = b.center;
        Quaternion targetDirection = Quaternion.LookRotation(centre - origin.position);
        Vector3 targetUp = targetDirection * Vector3.up;
        Vector3 targetRight = targetDirection * Vector3.right;
        Vector3 gridStartCorner = centre + (-targetRight * zoneWidth / 2) + (-targetUp * zoneHeight / 2);

        /*
        Bounds b = MiscFunctions.CombinedBounds(targetColliders);
        Vector3 centre = b.center;
        float maxExtent = b.extents.magnitude;
        Quaternion targetDirection = Quaternion.LookRotation(centre - origin.position);
        Vector3 targetUp = targetDirection * Vector3.up;
        Vector3 targetRight = targetDirection * Vector3.right;

        // Calculate the width and height of the zone to raycast in
        //float zoneWidth = Vector3.Distance(centre + (-targetRight * maxExtent), centre + (targetRight * maxExtent));
        //float zoneHeight = Vector3.Distance(centre + (-targetUp * maxExtent), centre + (targetUp * maxExtent));

        // Calculate the dimensions of the grid to raycast in
        int gridWidth = Mathf.CeilToInt(zoneWidth / raycastSpacing);
        int gridHeight = Mathf.CeilToInt(zoneHeight / raycastSpacing);

        float spacingX = zoneWidth / gridWidth;
        float spacingY = zoneHeight / gridHeight;
        
        Vector3 gridStartCorner = centre + (-targetRight * zoneWidth / 2) + (-targetUp * zoneHeight / 2);
        */
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Calculates raycast point to aim for
                Vector3 raycastHitDestination = gridStartCorner + (targetRight * spacingX * x) + (targetUp * spacingY * y);
                Vector3 raycastDirection = raycastHitDestination - origin.position;
                // If raycast hits within range and is inside the valid detection angle

                // Don't check in this direction if it's outside the AI's peripheral vision
                if (AngleCheck(raycastDirection, origin, angles) == false) continue;
                //if (Vector3.Angle(origin.forward, raycastDirection) >= angle) continue;

                // Run a line of sight check
                if (Physics.Raycast(origin.position, raycastDirection, out hit, range, viewDetection) == false) continue;

                if (MiscFunctions.ArrayContains(targetColliders, hit.collider) == false) continue;

                return true;
            }
        }

        hit = new RaycastHit();
        return false;
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
    /*
    public static Vector2 AngleThing(Vector3 extents, Vector3 direction)
    {
        // Idea: somehow rotate the extents as if it's a direction, towards direction? I'm pretty sure that should swap around the axes and scale them at the same time.
        //Vector3 newDir = Vector3.RotateTowards(extents, )
    }
    */


}
