using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimator : MonoBehaviour
{
    public Character character;

    [Header("Anatomy")]
    [SerializeField] Animator animator;
    [SerializeField] Ragdoll ragdoll;
    [SerializeField] Transform[] spineBones;

    [Header("Animator values")]
    [SerializeField] string walkXValue;
    [SerializeField] string walkZValue;
    
    /*
    void Start()
    {
        // Add listeners for other functions e.g. stagger animations
    }
    */
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
}
