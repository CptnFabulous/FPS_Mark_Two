using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PhysicsAffectedAI : MonoBehaviour
{
    [SerializeField] AI rootAI;
    [SerializeField] Rigidbody rigidbody;
    [SerializeField] CapsuleCollider collider;
    [SerializeField] Ragdoll ragdoll;

    NavMeshAgent navMeshAgent => rootAI.agent;
    public bool ragdollActive
    {
        get => ragdoll != null && ragdoll.enabled;
        set
        {
            if (ragdoll == null)
            {
                collider.enabled = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                return;
            }

            collider.enabled = !value;
            navMeshAgent.enabled = !value;
            ragdoll.enabled = value;
            // Freeze rotation when un-ragdolled to ensure the agent isn't constantly falling over
            rigidbody.constraints = !value ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
            // Perform unique functions upon ragdollising or returning to normal
            if (value)
            {
                // Unparent the ragdoll from the AI itself so the physics don't get wacky
                ragdoll.transform.SetParent(null);
                // Add navmeshagent velocity onto rigidbody velocity to preserve existing momentum
                rigidbody.velocity += navMeshAgent.velocity;
                navMeshAgent.velocity = Vector3.zero;
                // Update ragdoll velocity to inherit base rigidbody velocity
                Debug.Log($"Rigidbody velocity = {rigidbody.velocity}, angular velocity = {rigidbody.angularVelocity}");
                foreach (Rigidbody rb in ragdoll.rigidbodies)
                {
                    rb.velocity += rigidbody.velocity;
                    rb.angularVelocity += rigidbody.angularVelocity;
                }
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                Debug.Log($"Rigidbody velocity = {ragdoll.rootRigidbody.velocity}, angular velocity = {ragdoll.rootRigidbody.angularVelocity}");
            }
            else
            {
                // Secretly update the real AI's orientation to match the ragdoll's, then re-parent them.
                // This should hide that the AI and ragdoll were ever separated.
                rootAI.transform.position = ragdoll.transform.position;
                rootAI.transform.rotation = ragdoll.transform.rotation;
                ragdoll.transform.SetParent(transform);
            }
            rootAI.gameObject.SetActive(!value);
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
        // Don't bother updating anything if the agent is ragdolled
        if (ragdollActive) return;

        // Update physics collider size to match NavMeshAgent size
        collider.radius = navMeshAgent.radius;
        collider.height = navMeshAgent.height;
        
        // Instead of letting the agent directly modify the position, retrieve the desired movement changes and apply them to the rigidbody 
        Vector3 velocity = navMeshAgent.velocity;
        if (velocity.sqrMagnitude > 0)
        {
            Vector3 currentPosition = rigidbody.transform.position;
            navMeshAgent.nextPosition = currentPosition; // Ensure the navmesh's stored position always matches the object's real position in space
            rigidbody.MovePosition(currentPosition + (Time.fixedDeltaTime * velocity));
        }
    }
}
