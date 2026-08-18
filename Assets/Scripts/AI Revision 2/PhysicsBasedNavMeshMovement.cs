using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PhysicsBasedNavMeshMovement : MonoBehaviour
{
    [SerializeField] AI rootAI;
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] CapsuleCollider collider;
    [SerializeField] float groundingRayLength = 0.01f;

    NavMeshAgent navMeshAgent => rootAI.agent;

    private void OnEnable()
    {
        // Ensures the various functional elements are active
        rigidbody.isKinematic = false;
        navMeshAgent.updatePosition = false;
    }
    private void FixedUpdate()
    {
        // Update physics collider size and position to match agent's
        collider.radius = navMeshAgent.radius;
        collider.height = navMeshAgent.height;
        collider.center = new Vector3(0, navMeshAgent.height / 2, 0);

        // Check if AI is animated, and standing grounded on a valid NavMesh. Disable gravity if so.
        GroundingHandler.GetGroundingData(collider, groundingRayLength, out RaycastHit groundingData, out bool isGrounded);
        bool agentMoving = navMeshAgent.enabled && isGrounded && navMeshAgent.velocity.sqrMagnitude > 0;
        rigidbody.useGravity = !agentMoving;


        // Instead of letting the agent directly modify the position, retrieve the desired movement changes and apply them to the rigidbody
        Vector3 currentPosition = rigidbody.transform.position;
        navMeshAgent.nextPosition = currentPosition; // Ensure the navmesh's stored position always matches the object's real position in space
        Vector3 velocity = navMeshAgent.velocity;
        if (velocity.sqrMagnitude > 0)
        {
            rigidbody.MovePosition(currentPosition + (Time.fixedDeltaTime * velocity));
        }
    }
    private void OnDrawGizmos()
    {
        if (MiscFunctions.CurrentCameraNotMain()) return;

        Gizmos.color = Color.blue;
        AIAction.GizmosDrawNavMeshPath(navMeshAgent.path);

        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 centre = new Vector3(0, navMeshAgent.height / 2, 0);
        float width = navMeshAgent.radius * 2;
        Vector3 size = new Vector3(width, navMeshAgent.height, width);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(centre, size);
    }
}
