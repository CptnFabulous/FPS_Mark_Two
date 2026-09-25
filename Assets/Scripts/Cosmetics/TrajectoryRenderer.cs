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
    public float drag { get; set; }
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

        Projectile.ProjectileData data = new Projectile.ProjectileData();
        data.mass = mass;
        data.drag = drag;
        data.radius = radius;
        data.hitDetection = hitDetection;
        data.onTravel = () =>
        {
            //Debug.Log($"{positions.Length}, {positionCount}");
            if (positionCount >= maxVertexCount) return;
            positions[positionCount] = position;
            positionCount++;
        };
        data.onHit = (rh) =>
        {
            surfaceHit = true;
            thingHit = rh;
            return Projectile.ProjecileHitResult.Bounce;
        };

        for (int i = positionCount; i < maxVertexCount; i++)
        {
            float deltaTime = lengthPerSegment / velocity.magnitude;

            Projectile.CalculateTrajectoryDelta(ref position, ref velocity, lengthPerSegment, data);

            if (positionCount >= maxVertexCount) break;
            if (velocity.magnitude <= 0) break;
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
