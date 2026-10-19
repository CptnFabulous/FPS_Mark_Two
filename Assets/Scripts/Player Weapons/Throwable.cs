using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Throwable : MonoBehaviour
{
    [SerializeField] Collider _collider;
    [SerializeField] Rigidbody _rigidbody;

    public Collider collider => _collider;
    public Rigidbody rb => _rigidbody;

    public abstract void OnThrow();
    public abstract void Disable();


    private void Awake()
    {
        Disable();
    }
    private void OnDisable()
    {
        Disable();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, rb.velocity);
    }
}
