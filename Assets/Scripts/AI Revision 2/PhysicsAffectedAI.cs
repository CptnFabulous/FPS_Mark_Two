using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PhysicsAffectedAI : MonoBehaviour
{
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] Rigidbody rigidbody;

#if UNITY_EDITOR
    [SerializeField] GameObject target;

    private void Start()
    {
        navMeshAgent.destination = target.transform.position;
    }
    [ContextMenu("Reassign position")]
    void ReassignPosition()
    {
        navMeshAgent.destination = target.transform.position;
    }
#endif

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
