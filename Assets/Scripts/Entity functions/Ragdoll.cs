using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ragdoll : MonoBehaviour
{
    public Character attachedTo;
    public Transform rootBone;
    public SkinnedMeshRenderer baseRenderer;
    [SerializeField] Animator animator;
    [SerializeField] CollisionDetectionMode collisionDetectionMode;

    public UnityEvent<bool> onActiveStateSet;

    Rigidbody[] _rb;

    public Rigidbody[] rigidbodies => _rb ??= GetComponentsInChildren<Rigidbody>();
    public Rigidbody rootRigidbody => rigidbodies[0];
    public float combinedMass
    {
        get
        {
            float final = 0;
            foreach (Rigidbody rb in rigidbodies) final += rb.mass;
            return final;
        }
        set
        {
            float divided = value / rigidbodies.Length;
            foreach (Rigidbody rb in rigidbodies) rb.mass = divided;
        }
    }
    public Vector3 totalVelocity
    {
        get
        {
            Vector3 value = Vector3.zero;
            foreach (Rigidbody rb in rigidbodies) value += rb.velocity;
            return value / rigidbodies.Length;
        }
        set
        {
            foreach (Rigidbody rb in rigidbodies) rb.velocity = value;
        }
    }
    public Vector3 totalAngularVelocity
    {
        get
        {
            Vector3 value = Vector3.zero;
            foreach (Rigidbody rb in rigidbodies) value += rb.angularVelocity;
            return value / rigidbodies.Length;
        }
        set
        {
            foreach (Rigidbody rb in rigidbodies) rb.angularVelocity = value;
        }
    }

    private void Awake()
    {
        combinedMass = rootRigidbody.mass;
        SetActive(enabled);
    }
    private void OnEnable() => SetActive(true);
    private void OnDisable() => SetActive(false);
    void SetActive(bool active)
    {
        onActiveStateSet.Invoke(active);
        
        if (animator != null) animator.enabled = !active;
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !active;
            rb.constraints = RigidbodyConstraints.None;
            // Collision mode needs to be set to continuous speculative, when a rigidbody is kinematic
            rb.collisionDetectionMode = !active ? CollisionDetectionMode.ContinuousSpeculative : collisionDetectionMode;
        }

        if (baseRenderer != null && active == false)
        {
            baseRenderer.transform.localPosition = Vector3.zero;
            baseRenderer.transform.localRotation = Quaternion.identity;
        }
    }
}
