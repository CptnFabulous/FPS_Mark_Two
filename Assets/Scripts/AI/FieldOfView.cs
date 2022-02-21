using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{

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
            

            List<Collider> objects = new List<Collider>(Physics.OverlapSphere(viewAxis.position, viewRange, viewDetection));
            for (int i = 0; i < objects.Count; i++)
            {
                #region Angle check
                float distance = Vector3.Distance(viewAxis.position, objects[i].bounds.center);
                Vector3 roughlyAdjacentPosition = viewAxis.position + (viewAxis.forward * distance);
                Vector3 closestPoint = objects[i].ClosestPoint(roughlyAdjacentPosition);
                // Produces a quaternion pointing from the look origin towards the closest point
                Quaternion closestPointDirectionRotation = Quaternion.LookRotation(closestPoint - viewAxis.position, viewAxis.up);
                // Makes that quaternion relative to the direction the AI is looking in
                Quaternion differenceRotation = viewAxis.rotation * Quaternion.Inverse(closestPointDirectionRotation);
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



    //


    /// <summary>
    /// Calculates and runs a sweep of raycasts to check a cone-shaped area for colliders. This check will also detect partially-hidden colliders, but is performance-intensive and should not be run regularly.
    /// </summary>
    /// <param name="targetColliders"></param>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="angle"></param>
    /// <param name="range"></param>
    /// <param name="hit"></param>
    /// <param name="viewDetection"></param>
    /// <param name="raycastSpacing"></param>
    /// <returns></returns>
    public static bool ComplexDetectionConeCheck(Collider[] targetColliders, Vector3 origin, Vector3 direction, float angle, float range, out RaycastHit hit, LayerMask viewDetection, float raycastSpacing = 0.25f)
    {
        Bounds b = MiscFunctions.CombinedBounds(targetColliders);
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

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Calculates raycast point to aim for
                Vector3 raycastHitDestination = gridStartCorner + (right * spacingX * x) + (up * spacingY * y);
                Vector3 raycastDirection = raycastHitDestination - origin;
                // If raycast hits within range and is inside the valid detection angle
                if (Vector3.Angle(direction, raycastDirection) < angle && Physics.Raycast(origin, raycastDirection, out hit, range, viewDetection))
                {
                    if (MiscFunctions.ArrayContains(targetColliders, hit.collider))
                    {
                        Debug.DrawRay(origin, direction.normalized * range, Color.green, 3);
                        return true;
                    }
                    else
                    {
                        Debug.DrawRay(origin, direction.normalized * range, Color.yellow, 3);
                    }
                }
                else
                {
                    Debug.DrawRay(origin, direction.normalized * range, Color.red, 3);
                }
            }
        }

        hit = new RaycastHit();
        return false;
    }





    private void OnDrawGizmos()
    {
        Gizmos.matrix = viewAxis.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, viewingAngles.y, viewRange, 0, viewingAngles.x / viewingAngles.y);
    }
}
