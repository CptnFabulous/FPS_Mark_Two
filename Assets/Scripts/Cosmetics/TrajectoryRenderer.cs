using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform reticleEndTransform;
    [SerializeField] int maxVertexCount = 100;
    [SerializeField] float lengthPerSegment;
    [SerializeField] string velocityMaterialProperty = "_Velocity";

    Vector3[] positions;

    public System.Func<(Vector3, Vector3)> getStartPositionAndVelocity { get; set; }
    public float mass { get; set; }
    public LayerMask hitDetection { get; set; }
    
    private void Awake()
    {
        lineRenderer.useWorldSpace = true;
        positions = new Vector3[maxVertexCount];
    }
    private void LateUpdate()
    {
        // Cancel if there's no data assigned.
        if (getStartPositionAndVelocity == null) return;

        // Get starting position and velocity. Don't proceed if there's no velocity to form a trajectory.
        (Vector3, Vector3) startValues = getStartPositionAndVelocity.Invoke();
        Vector3 startPosition = startValues.Item1;
        Vector3 startVelocity = startValues.Item2;
        if (startVelocity.magnitude <= 0) return;

        bool surfaceHit = false;
        RaycastHit thingHit = new RaycastHit();
        Vector3 position = startPosition;
        Vector3 velocity = startVelocity;

        float radius = lineRenderer.widthMultiplier / 2;

        positions[0] = position;

        int positionCount = 1;
        for (positionCount = 1; positionCount < maxVertexCount; positionCount++)
        {
            float deltaTime = lengthPerSegment / velocity.magnitude;
            surfaceHit = Projectile.CalculateTrajectoryDelta(ref position, ref velocity, mass, radius, deltaTime, hitDetection, out thingHit);

            // If something is hit, we've reached the end of the trajectory.
            if (surfaceHit)
            {
                positions[positionCount] = thingHit.point;
                positionCount += 1;
                break;
            }

            // If nothing is hit, return the next position along the trajectory.
            positions[positionCount] = position;
        }
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPositions(positions);

        // Set material animation speed to match velocity
        lineRenderer.material.SetFloat(velocityMaterialProperty, startVelocity.magnitude);

        // Enable/disable and orient reticle for point of impact
        reticleEndTransform.gameObject.SetActive(surfaceHit);
        if (surfaceHit)
        {
            reticleEndTransform.position = thingHit.point;
            reticleEndTransform.rotation = Quaternion.LookRotation(-thingHit.normal, transform.up);
        }
    }
}
