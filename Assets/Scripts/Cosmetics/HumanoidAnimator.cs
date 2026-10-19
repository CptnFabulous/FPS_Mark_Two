using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimator : MonoBehaviour
{
    public Character character;

    [Header("Anatomy")]
    [SerializeField] public Animator animator;
    [SerializeField] Transform[] spineBones;

    [Header("Movement")]
    [SerializeField] string standardMovementLayer = "Movement";
    [SerializeField] string walkXValue = "Movement X";
    [SerializeField] string walkZValue = "Movement Z";
    [SerializeField] ImpactEffect footsteps;

    [Header("Stuns")]
    [SerializeField] string damageDirectionX = "Damage Direction X";
    [SerializeField] string damageDirectionZ = "Damage Direction Z";

    [Header("Ragdoll")]
    [SerializeField] string standUpState = "Movement.Stand up from fall";
    [SerializeField] string ragdollOrientationDotProduct = "Ragdoll orientation dot product";

    PhysicsAffectedAI physicsHandler => (character as AI).physicsHandler;
    //public Ragdoll ragdoll => character.ragdoll;
    public int defaultAnimationLayer => animator.GetLayerIndex(standardMovementLayer);

    private void Awake()
    {
        if (character != null)
        {
            if (character != null && character.health != null) character.health.onDamage.AddListener(UpdateDamageData);
            //Debug.Log($"{this}, {character}");
            //Debug.Log($"{ragdoll}");
            //ragdoll.onActiveStateSet.AddListener((active) => animator.enabled = !active);
        }
        else
        {
            enabled = false;
        }
        
    }
    private void Update()
    {
        //if (ragdoll.enabled) return;

        // Update walk direction values
        Vector3 walkValues = character.LocalMovementDirection;
        animator.SetFloat(walkXValue, walkValues.x);
        animator.SetFloat(walkZValue, walkValues.z);
    }
    /*
    void LateUpdate()
    {
        if (character == null)
        {
            enabled = false;
            return;
        }

        if (ragdoll.enabled) return;


        if (character.lookController.active)
        {
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
    }
    */

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
    [Obsolete]
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
    public void PlayFootstep()
    {
        // Currently it just plays a noise despite using an ImpactEffect as a parameter.
        // Later on, I want to add code for things like displaying footstep decals
        footsteps.defaultSound.Play(transform.position, character, 1);
        //footsteps.Play()
    }

    public static void CalculateBasePositionBasedOnRagdoll(Transform baseTransform, Transform rootBone, Bounds ragdollBounds, out Vector3 position, out Quaternion rotation, out float dot)
    {
        Vector3 up = baseTransform.up;//-Physics.gravity;

        // Check what direction the enemy would face after standing up, based on their ragdoll direction
        // If we compare the direction to up and it's zero, that means it's perfectly forward.
        Vector3 ragdollForward = rootBone.forward;
        dot = Vector3.Dot(rootBone.forward, baseTransform.up);
        if (dot < 0)
        {
            // Less than 0 means ragdoll is closer to lying on its face. Use the 'up' vector instead of 'forward'
            ragdollForward = rootBone.up;
        }
        else if (dot > 0)
        {
            // More than 0 means ragdoll is closer to lying on its back. Use the 'down' vector instead of 'forward'
            ragdollForward = -rootBone.up;
        }

        // Calculate the direction the AI should face in to roughly match the ragdoll (while remaining upright)
        Vector3 aiForward = Vector3.ProjectOnPlane(ragdollForward, up);

        // Adjust transform to be roughly in the centre-bottom of the ragdoll's bounds
        // TO DO: have code that converts ragdoll bounds to the AI's local space instead of world space

        Vector3 bottom = ragdollBounds.center;
        bottom.y = ragdollBounds.min.y;
        position = bottom;


        // Adjust direction to face roughly in the same 'forward' direction as the ragdoll
        rotation = Quaternion.LookRotation(aiForward, up);
    }
}