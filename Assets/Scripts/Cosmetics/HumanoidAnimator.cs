using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimator : MonoBehaviour
{
    public Character character;

    [Header("Anatomy")]
    [SerializeField] Animator animator;
    [SerializeField] PhysicsAffectedAI physicsHandler;
    [SerializeField] Transform[] spineBones;

    [Header("Animator values")]
    [SerializeField] string standardMovementLayer = "Movement";
    [SerializeField] string walkXValue = "Movement X";
    [SerializeField] string walkZValue = "Movement Z";

    [Header("Stuns")]
    [SerializeField] string damageDirectionX = "Damage Direction X";
    [SerializeField] string damageDirectionZ = "Damage Direction Z";

    [SerializeField] string standUpState = "Movement.Stand up from fall";
    [SerializeField] string ragdollOrientationDotProduct = "Ragdoll orientation dot product";

    public Ragdoll ragdoll => physicsHandler.ragdoll;
    public int defaultAnimationLayer => animator.GetLayerIndex(standardMovementLayer);

    private void Awake()
    {
        character.health.onDamage.AddListener(UpdateDamageData);
        /*
        ragdoll.onActiveStateSet.AddListener((b) =>
        {
            if (b == false) RecoverFromRagdoll();
        });
        */
        ragdoll.onActiveStateSet.AddListener((active) => animator.enabled = !active);
    }
    private void Update()
    {
        if (ragdoll.enabled) return;

        // Update walk direction values
        Vector3 walkValues = character.LocalMovementDirection;
        animator.SetFloat(walkXValue, walkValues.x);
        animator.SetFloat(walkZValue, walkValues.z);
    }
    void LateUpdate()
    {
        if (ragdoll.enabled) return;

        // Set up appropriate aiming rotation of upper body
        // I used this tutorial for reference https://www.youtube.com/watch?v=Q56quIB2sOg

        // Generates an offset rotation so that the upper body transform rotates in the appropriate direction of the head
        Quaternion rotationOffsetBetweenAIAndBody = Quaternion.FromToRotation(character.transform.forward, character.LookTransform.forward);
        for (int i = 0; i < spineBones.Length; i++)
        {
            float value = (i + 1) / spineBones.Length;
            // Creates weighted rotation that increases towards the end of the array (last bone is fully weighted)
            Quaternion rotation = Quaternion.Slerp(Quaternion.identity, rotationOffsetBetweenAIAndBody, value);
            spineBones[i].rotation = rotation * spineBones[i].rotation;
        }
    }

    void UpdateDamageData(DamageMessage damageMessage)
    {
        Vector3 localDamageDirection = transform.InverseTransformDirection(damageMessage.direction);
        localDamageDirection = localDamageDirection.normalized;
        animator.SetFloat(damageDirectionX, localDamageDirection.x);
        animator.SetFloat(damageDirectionZ, localDamageDirection.z);
    }

    /// <summary>
    /// Run this animation when standing up from being ragdollised. Overrides existing states and ensure the animator goes right to the correct animation
    /// </summary>
    public void RecoverFromRagdoll()
    {
        /*
        animator.SetTrigger(standUpTrigger);
        animator.Update(0);
        return;
        */
        float dotProductSign = Mathf.Sign(physicsHandler.ragdollUprightDotProduct);
        animator.SetFloat(ragdollOrientationDotProduct, dotProductSign);
        animator.Play(standUpState, defaultAnimationLayer, 0);
    }
}
