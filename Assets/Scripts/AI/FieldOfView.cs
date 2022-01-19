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

    /// <summary>
    /// Calculates and runs a sweep of raycasts to check an area for sections of a particular collider. This is performance-intensive and should not be run regularly.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="raycastSpacing"></param>
    /// <param name="hit"></param>
    /// <param name="viewDetection"></param>
    /// <returns></returns>
    /// 
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



    /*
    public Collider testCollider;
    public LayerMask mask = ~0;

    float lastTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Time.time - lastTime > 3)
        {
            if (FieldOfView.ComplexLineOfSightCheck(transform.position, testCollider, 0.25f, out RaycastHit hit, mask))
            {
                Debug.DrawRay(testCollider.bounds.center, Vector3.up * 5, Color.magenta, 3);
            }
            
            lastTime = Time.time;
        }
        
    }
    */


    private void OnDrawGizmos()
    {
        Gizmos.matrix = viewAxis.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, viewingAngles.y, viewRange, 0, viewingAngles.x / viewingAngles.y);
    }
}
