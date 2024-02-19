using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] CollisionDetectionMode collisionDetectionMode;

    
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
    private void Awake()
    {
        SetActive(enabled);
    }
    private void OnEnable() => SetActive(true);
    private void OnDisable() => SetActive(false);
    void SetActive(bool active)
    {
        if (animator != null) animator.enabled = !active;
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !active;
            rb.constraints = RigidbodyConstraints.None;
            // If kinematic, the collision mode has to be continuous speculative. Otherwise set it to whatever it's meant to be normally.
            rb.collisionDetectionMode = !active ? CollisionDetectionMode.ContinuousSpeculative : collisionDetectionMode;
        }
    }
}
