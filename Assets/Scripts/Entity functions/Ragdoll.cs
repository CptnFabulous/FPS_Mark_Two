using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ragdoll : MonoBehaviour
{
    public Entity attachedTo;
    public Transform rootBone;
    [SerializeField] Transform[] _boneTransforms;

    [Header("Logic")]
    [SerializeField] CollisionDetectionMode collisionDetectionMode;
    public UnityEvent<bool> onActiveStateSet;

    [Header("Animations")]
    public SkinnedMeshRenderer baseRenderer;

    Rigidbody[] _rb;
    Vector3 rootBoneOriginalPosition;
    Quaternion rootBoneOriginalRotation;

    public Transform[] boneTransforms => _boneTransforms;
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
        rootBoneOriginalPosition = rootBone.localPosition;
        rootBoneOriginalRotation = rootBone.localRotation;
        
        combinedMass = rootRigidbody.mass;
        SetActive(enabled);
    }
    private void OnEnable() => SetActive(true);
    private void OnDisable() => SetActive(false);
    void SetActive(bool active)
    {
        onActiveStateSet.Invoke(active);

        // When activating ragdoll, ensure root bone is at the default orientation relative to the base transform.
        // Otherwise it causes stretching issues, as the joints are synced up to the correct position.
        if (active)
        {
            // There seems to sometimes be an issue where the ragdoll is rotated weirdly, but it's too small and rare to easily notice.
            
            Transform parent = rootBone.parent;
            
            // Get world position/rotation of root bone
            Vector3 worldPos = rootBone.position;
            Quaternion worldRot = rootBone.rotation;
            // Set local position/rotation to origin
            rootBone.localPosition = rootBoneOriginalPosition;
            rootBone.localRotation = rootBoneOriginalRotation;
            // Get difference between current and desired rotation, and add to parent
            Quaternion rotOffset = worldRot * Quaternion.Inverse(rootBone.rotation);
            parent.rotation *= rotOffset;
            // ONCE ROTATED: get difference between current and desired, and add to parent
            Vector3 posOffset = worldPos - rootBone.position;
            parent.position += posOffset;
            
            /*
            // Get offset between root bone's desired and current orientation (in world space)
            Vector3 positionOffset = rootBone.position - parent.TransformPoint(rootBoneOriginalPosition);
            Quaternion rotationOffset = rootBone.rotation * Quaternion.Inverse(rootBoneOriginalRotation);
            // Shift root bone to desired local orientation
            rootBone.localPosition = rootBoneOriginalPosition;
            rootBone.localRotation = rootBoneOriginalRotation;
            // Apply reverse offset on parent, so root bone is still in the same world orientation as before
            parent.position += positionOffset;
            parent.rotation *= rotationOffset;
            */
        }

        // Set kinematic states of all rigidbodies
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !active;
            rb.constraints = RigidbodyConstraints.None;
            // Collision mode needs to be set to continuous speculative, when a rigidbody is kinematic
            rb.collisionDetectionMode = !active ? CollisionDetectionMode.ContinuousSpeculative : collisionDetectionMode;
        }

        // Reset and realign renderer position
        if (baseRenderer != null && active == false)
        {
            baseRenderer.transform.localPosition = Vector3.zero;
            baseRenderer.transform.localRotation = Quaternion.identity;
        }
    }
}
