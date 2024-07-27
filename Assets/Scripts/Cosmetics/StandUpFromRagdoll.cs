using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandUpFromRagdoll : MonoBehaviour
{
    public Ragdoll ragdoll;
    public float lerpDuration = 1f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Vector3[] bonePositions;
    Quaternion[] boneRotations;
    float enterTime;

    public Transform[] ragdollBones => ragdoll.boneTransforms;
    int boneCount => ragdollBones.Length;
    float timeElapsed => Time.time - enterTime;
    
    public void StartTransition()
    {
        Debug.Log("Starting standup transtion");
        enterTime = Time.time;

        // Create new arrays and cache the orientation of each ragdoll bone
        bonePositions = new Vector3[boneCount];
        boneRotations = new Quaternion[boneCount];
        for (int i = 0; i < boneCount; i++)
        {
            Transform rbt = ragdollBones[i];
            bonePositions[i] = rbt.localPosition;
            boneRotations[i] = rbt.localRotation;
        }

        enabled = true;
    }

    public void LateUpdate()
    {
        if (bonePositions == null) return;
        if (boneRotations == null) return;

        float t = transitionCurve.Evaluate(timeElapsed / lerpDuration);
        for (int i = 0; i < boneCount; i++)
        {
            // Each frame, the animator will forcefully override the current orientation to match the current animation frame.
            // Replace that orientation to a lerp value, to it from the cached orientation.
            Transform rbt = ragdollBones[i];
            rbt.localPosition = Vector3.Lerp(bonePositions[i], rbt.localPosition, t);
            rbt.localRotation = Quaternion.Lerp(boneRotations[i], rbt.localRotation, t);
        }
    }
}