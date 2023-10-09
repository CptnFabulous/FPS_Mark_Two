using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Throwable : MonoBehaviour
{
    [SerializeField] Collider _collider;
    [SerializeField] Rigidbody _rigidbody;

    public Collider collider => _collider;
    public Rigidbody rb => _rigidbody;

    private void Awake()
    {
        rb.isKinematic = true;
        collider.enabled = false;
    }
    public void TriggerThrow(Vector3 throwForce)
    {
        enabled = true;
        transform.SetParent(null, true);
        rb.isKinematic = false;
        collider.enabled = true;
        rb.AddForce(throwForce, ForceMode.Impulse);
        OnThrow();
    }
    public abstract void OnThrow();

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(transform.position, rb.velocity);
    }
}
