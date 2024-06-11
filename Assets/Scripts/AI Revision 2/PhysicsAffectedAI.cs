using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PhysicsAffectedAI : MonoBehaviour
{
    [SerializeField] AI rootAI;
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] CapsuleCollider collider;
    public Ragdoll ragdoll;

    NavMeshAgent navMeshAgent => rootAI.agent;
    public bool ragdollActive
    {
        get => ragdoll != null && ragdoll.enabled;
        set => StartCoroutine(SetRagdollActiveState(value));
    }

    public IEnumerator SetRagdollActiveState(bool value)
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
        collider.enabled = !value;
        navMeshAgent.enabled = !value;
        ragdoll.enabled = value;

        // Ensure the root AI doesn't fall through the floor
        // Since when all its colliders are disabled/separated due to turning into a ragdoll, there's nothing stopping it from doing so
        rigidbody.useGravity = !value;

        // Perform unique functions upon ragdollising or returning to normal
        if (value)
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
            // Secretly update the real AI's orientation to match the ragdoll's (while preserving the AI's up axis), then re-parent them.
            // This should hide that the AI and ragdoll were ever separated.
            UpdateBasePositionToMatchRagdoll();

            // Transfer the ragdoll velocity back to the base rigidbody
            rigidbody.velocity = ragdoll.totalVelocity;
            rigidbody.angularVelocity = ragdoll.totalAngularVelocity;
            ragdoll.totalVelocity = Vector3.zero;
            ragdoll.totalAngularVelocity = Vector3.zero;

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
        // If the ragdoll is active, ensure the AI's orientation lines up with the ragdoll's (they're only separated so the physics don't bug out)
        if (ragdollActive)
        {
            UpdateBasePositionToMatchRagdoll();
            return;
        }

        // Update physics collider size to match NavMeshAgent size
        collider.radius = navMeshAgent.radius;
        collider.height = navMeshAgent.height;

        // Instead of letting the agent directly modify the position, retrieve the desired movement changes and apply them to the rigidbody
        Vector3 currentPosition = rigidbody.transform.position;
        navMeshAgent.nextPosition = currentPosition; // Ensure the navmesh's stored position always matches the object's real position in space
        Vector3 velocity = navMeshAgent.velocity;
        if (velocity.sqrMagnitude > 0)
        {
            rigidbody.MovePosition(currentPosition + (Time.fixedDeltaTime * velocity));
        }
    }

    public float ragdollUprightDotProduct => Vector3.Dot(ragdoll.rootBone.forward, rootAI.transform.up);

    void UpdateBasePositionToMatchRagdoll()
    {
        Transform aiTransform = rootAI.transform;
        Transform ragdollRootBone = ragdoll.rootBone;

        Vector3 up = aiTransform.up;//-Physics.gravity;
        
        // Check what direction the enemy would face after standing up, based on their ragdoll direction
        // If we compare the direction to up and it's zero, that means it's perfectly forward.
        // More than 0 means ragdoll is closer to lying on its back. Use the 'down' vector
        // Less than 0 means ragdoll is closer to lying on its face. Use the 'up' vector
        Vector3 ragdollForward = ragdollRootBone.forward;
        float dot = ragdollUprightDotProduct;
        if (dot < 0)
        {
            ragdollForward = ragdollRootBone.up;
        }
        else if (dot > 0)
        {
            ragdollForward = -ragdollRootBone.up;
        }

        // Calculate the direction the AI should face in to roughly match the ragdoll (while remaining upright)
        Vector3 aiForward = Vector3.ProjectOnPlane(ragdollForward, up);
        // Adjust transform to be roughly in the centre of the ragdoll's body
        aiTransform.position = rootAI.bounds.center;
        // Adjust direction to face roughly in the same 'forward' direction as the ragdoll
        aiTransform.rotation = Quaternion.LookRotation(aiForward, up);
    }
}
