using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundingHandler : MonoBehaviour
{
    public float groundingRayLength = 0.01f;
    public CapsuleCollider collider;
    public UnityEvent<RaycastHit> onLand;

    public RaycastHit groundingData { get; private set; }

    public bool isGrounded => groundingData.collider != null;

    private void FixedUpdate()
    {
        GetGroundingData(collider, groundingRayLength, out RaycastHit newGroundingData);
        if (newGroundingData.collider != null && groundingData.collider == null)
        {
            onLand.Invoke(newGroundingData);
        }
        groundingData = newGroundingData; // Update grounding data
    }

    public static void GetGroundingData(CapsuleCollider collider, float groundingRayLength, out RaycastHit newGroundingData)
    {
        Transform transform = collider.transform;
        LayerMask collisionMask = MiscFunctions.GetPhysicsLayerMask(collider.gameObject.layer);

        Vector3 rayOrigin = transform.position + transform.up * (collider.height / 2);
        float distance = groundingRayLength + Vector3.Distance(transform.position, rayOrigin);
        float radius = collider.radius * 0.9f;
        Physics.SphereCast(rayOrigin, radius, -transform.up, out newGroundingData, distance, collisionMask);
    }
}