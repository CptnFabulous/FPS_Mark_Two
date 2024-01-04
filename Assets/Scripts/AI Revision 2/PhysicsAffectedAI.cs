using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PhysicsAffectedAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] Rigidbody rigidbody;

    private void OnEnable()
    {
        navMeshAgent.updatePosition = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        rigidbody.isKinematic = false;
    }
    private void FixedUpdate()
    {
        Vector3 velocity = navMeshAgent.velocity;
        if (velocity.sqrMagnitude > 0)
        {
            Vector3 currentPosition = rigidbody.transform.position;
            navMeshAgent.nextPosition = currentPosition;
            rigidbody.MovePosition(currentPosition + (Time.fixedDeltaTime * velocity));
        }
    }
}
