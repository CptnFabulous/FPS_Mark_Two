using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class PhysicsAffectedAI : MonoBehaviour
{
    [SerializeField] AI rootAI;
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] CapsuleCollider collider;
    [SerializeField] float groundingRayLength = 0.01f;
    public Ragdoll ragdoll;

    NavMeshAgent navMeshAgent => rootAI.agent;
    public bool ragdollActive
    {
        get => ragdoll != null && ragdoll.enabled;
        set => ragdoll.StartCoroutine(SetRagdollActiveState(value));
    }
    public float ragdollUprightDotProduct => Vector3.Dot(ragdoll.rootBone.forward, rootAI.transform.up);

    public IEnumerator SetRagdollActiveState(bool active)
    {
        // Don't bother setting everything up if there isn't a ragdoll, just set it to the regular active state
        if (ragdoll == null)
        {
            collider.enabled = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            yield break;
        }

        // Wait for the next fixed update call, so that all physics changes have occurred properly
        yield return new WaitForFixedUpdate();

        // Set active states of appropriate scripts
        collider.enabled = !active;
        navMeshAgent.enabled = !active;
        //rootAI.animator.enabled = !value;
        ragdoll.enabled = active;

        // Ensure the root AI doesn't fall through the floor
        // Since when all its colliders are disabled/separated due to turning into a ragdoll, there's nothing stopping it from doing so
        rigidbody.useGravity = !active;

        // Perform unique functions upon ragdollising or returning to normal
        if (active)
        {
            // Unparent the ragdoll from the AI itself so the physics don't get wacky
            ragdoll.transform.SetParent(null);

            // Add navmeshagent velocity onto rigidbody velocity to preserve existing momentum
            rigidbody.velocity += navMeshAgent.velocity;
            navMeshAgent.velocity = Vector3.zero;

            // Update ragdoll velocity to inherit base rigidbody velocity
            ragdoll.totalVelocity += rigidbody.velocity;
            ragdoll.totalAngularVelocity += rigidbody.angularVelocity;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            // Store prior look direction (so it can be reassigned afterwards)
            Quaternion lookRotation = rootAI.aiming.lookRotation;

            // Secretly update the real AI's orientation to match the ragdoll's (while preserving the AI's up axis), then re-parent them.
            // This should hide that the AI and ragdoll were ever separated.
            UpdateBasePositionToMatchRagdoll();

            // Transfer the ragdoll velocity back to the base rigidbody
            rigidbody.velocity = ragdoll.totalVelocity;
            rigidbody.angularVelocity = ragdoll.totalAngularVelocity;
            //ragdoll.totalVelocity = Vector3.zero;
            //ragdoll.totalAngularVelocity = Vector3.zero;

            // Reset the ragdoll's transform to zero, but preserve the position of the base bone relative to the AI itself (for animation purposes)
            Transform ragdollTransform = ragdoll.transform;
            Transform ragdollRootBone = ragdoll.rootBone;

            // Preserve current position of root bone
            Vector3 rootBoneWorldPosition = ragdollRootBone.position;
            Quaternion rootBoneWorldRotation = ragdollRootBone.rotation;

            // Re-assign ragdoll parent and reset its transform orientation
            ragdollTransform.SetParent(transform);
            ragdollTransform.localPosition = Vector3.zero;
            ragdollTransform.localRotation = Quaternion.identity;
            ragdollTransform.localScale = Vector3.one;

            // Re-assign root bone orientation (so the ragdoll position doesn't visually change)
            ragdollRootBone.position = rootBoneWorldPosition;
            ragdollRootBone.rotation = rootBoneWorldRotation;

            // Preserve prior look direction (without this the AI sometimes rotates to weird unintended directions while de-ragdollising)
            rootAI.aiming.lookRotation = lookRotation;
        }
    }
    private void OnEnable()
    {
        // Ensures the various functional elements are active
        rigidbody.isKinematic = false;
        navMeshAgent.updatePosition = false;
        ragdollActive = false;
        // Ensure the total mass of the ragdoll and capsule collider are the same
        if (ragdoll != null) ragdoll.combinedMass = rigidbody.mass;
    }
    private void FixedUpdate()
    {
        // Update physics collider size to match NavMeshAgent size
        collider.radius = navMeshAgent.radius;
        collider.height = navMeshAgent.height;

        // Check if AI is animated, and standing grounded on a valid NavMesh. Disable gravity if so.
        MovementController.GetGroundingData(collider, groundingRayLength, out RaycastHit groundingData);
        bool agentMoving = !ragdollActive && groundingData.collider != null && navMeshAgent.velocity.sqrMagnitude > 0;
        rigidbody.useGravity = !agentMoving;

        // If the ragdoll is active, ensure the AI's orientation lines up with the ragdoll's (they're only separated so the physics don't bug out)
        if (ragdollActive)
        {
            UpdateBasePositionToMatchRagdoll();
            return;
        }

        // Instead of letting the agent directly modify the position, retrieve the desired movement changes and apply them to the rigidbody
        Vector3 currentPosition = rigidbody.transform.position;
        navMeshAgent.nextPosition = currentPosition; // Ensure the navmesh's stored position always matches the object's real position in space
        Vector3 velocity = navMeshAgent.velocity;
        if (velocity.sqrMagnitude > 0)
        {
            rigidbody.MovePosition(currentPosition + (Time.fixedDeltaTime * velocity));
        }
    }

    void UpdateBasePositionToMatchRagdoll()
    {
        Transform aiTransform = rootAI.transform;
        Transform rootBone = ragdoll.rootBone;

        Vector3 up = aiTransform.up;//-Physics.gravity;
        
        // Check what direction the enemy would face after standing up, based on their ragdoll direction
        // If we compare the direction to up and it's zero, that means it's perfectly forward.
        Vector3 ragdollForward = rootBone.forward;
        float dot = ragdollUprightDotProduct;
        if (dot < 0)
        {
            // Less than 0 means ragdoll is closer to lying on its face. Use the 'up' vector instead of 'forward'
            ragdollForward = rootBone.up;
        }
        else if (dot > 0)
        {
            // More than 0 means ragdoll is closer to lying on its back. Use the 'down' vector instead of 'forward'
            ragdollForward = -rootBone.up;
        }

        // Calculate the direction the AI should face in to roughly match the ragdoll (while remaining upright)
        Vector3 aiForward = Vector3.ProjectOnPlane(ragdollForward, up);

        // Adjust transform to be roughly in the centre-bottom of the ragdoll's bounds
        // TO DO: have code that converts ragdoll bounds to the AI's local space instead of world space
        Bounds bounds = rootAI.bounds;
        Vector3 bottom = bounds.center;
        bottom.y = bounds.min.y;
        aiTransform.position = bottom;
        

        // Adjust direction to face roughly in the same 'forward' direction as the ragdoll
        aiTransform.rotation = Quaternion.LookRotation(aiForward, up);
    }
    public void SetPositionWithoutAdjustingRagdoll(Vector3 worldPosition)
    {
        Transform aiTransform = rootAI.transform;
        Transform rootBone = ragdoll.rootBone;

        // Preserve current position of root bone
        Vector3 rootBoneWorldPosition = rootBone.position;
        Quaternion rootBoneWorldRotation = rootBone.rotation;

        Debug.DrawLine(aiTransform.position, worldPosition, Color.cyan, 5);

        aiTransform.position = worldPosition;

        // Re-assign root bone orientation (so the ragdoll position doesn't visually change)
        rootBone.position = rootBoneWorldPosition;
        rootBone.rotation = rootBoneWorldRotation;
    }
}
