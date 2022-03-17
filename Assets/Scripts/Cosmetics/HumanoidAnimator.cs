using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimator : MonoBehaviour
{
    public Animator animator;
    public Character character;

    [Header("Variables")]
    public Transform[] spineBones;

    public string walkXValue;
    public string walkZValue;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        // Update walk direction values
        Vector3 walkValues = character.LocalMovementDirection;
        animator.SetFloat(walkXValue, walkValues.x);
        animator.SetFloat(walkZValue, walkValues.z);
    }

    void LateUpdate()
    {
        

        // Target position
        // Current aim transform (AimTransform)


        //Vector3 targetPosition;

        //Vector3 targetDirection = targetPosition - character.LookTransform.position;
        //Quaternion aimTowards = Quaternion.FromToRotation(character.LookTransform.forward, targetDirection);


        // Set up appropriate aiming rotation of upper body
        // Use this tutorial for reference https://www.youtube.com/watch?v=Q56quIB2sOg

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
