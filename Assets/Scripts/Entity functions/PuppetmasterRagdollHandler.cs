using CptnFabulous.MiscUtility;
using RootMotion;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PuppetmasterRagdollHandler : MonoBehaviour
{
    public AI rootAI;
    public PuppetMaster puppetmaster;
    [SerializeField] Collider centralCollider;
    [SerializeField] Rigidbody centralRigidbody;
    [SerializeField] Transform baseTransform;
    [SerializeField] Transform rootBone;
    [SerializeField] IK[] ikComponents;
    

    [Header("Ragdollising")]
    [SerializeField] public float collapseTime = 0.1f;
    [SerializeField] AnimationCurve weightDecayCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Checking if valid to stand up")]
    [SerializeField] float navMeshCheckDistance = 1f;
    [SerializeField] Collider[] colliders;

    [Header("Standing up")]
    public Animator animator;
    [SerializeField] string standUpTrigger = "Standing up";
    [SerializeField] string ragdollOrientationDotProduct = "Ragdoll orientation dot product";
    [SerializeField] float standUpTime = 4;
    [SerializeField] AnimationCurve puppetmasterWeightCurve = AnimationCurve.EaseInOut(0, 0, 2, 1);
    [SerializeField] AnimationCurve ikWeightCurve = AnimationCurve.EaseInOut(3.5f, 0, 4, 1);


    /*
    private void Awake()
    {
        float weightFraction = centralRigidbody.mass / puppetmaster.muscles.Length;
        foreach (Muscle muscle in puppetmaster.muscles)
        {
            muscle.rigidbody.mass = weightFraction;
        }
    }
    */


    

    private void FixedUpdate()
    {
        float forceTransferMultiplier = 1f;
        foreach (Muscle muscle in puppetmaster.muscles)
        {
            Vector3 accumulatedForce = muscle.rigidbody.GetAccumulatedForce();
            Vector3 accumulatedTorque = muscle.rigidbody.GetAccumulatedTorque();
            // If accumulated force/torque is greater than zero, add a portion of it to the central rigidbody
            centralRigidbody.AddForce(forceTransferMultiplier * accumulatedForce);
            centralRigidbody.AddTorque(forceTransferMultiplier * accumulatedTorque);
        }
    }


#if UNITY_EDITOR
    [ContextMenu("Ragdollise")]
    public void Ragdollise()
    {
        if (Application.isPlaying) StartCoroutine(CollapseAsync());
    }
    [ContextMenu("Resurrect")]
    public void Resurrect()
    {
        if (Application.isPlaying) StartCoroutine(StandUpAsync());
    }
#endif

    public IEnumerator CollapseAsync()
    {
        // Disable AI functionality
        SetAIFunctionsActive(false);
        // Start ragdolling
        SetRagdollActive(false);
        // Reduce puppet and IK weights to zero over a short period
        yield return MiscFunctions.WaitOnLerp(collapseTime, (ref float t) =>
        {
            float interpolator = weightDecayCurve.Evaluate(t);
            puppetmaster.muscleWeight = interpolator;
            puppetmaster.pinWeight = interpolator;
            SetIKWeight(interpolator);
        });
    }
    public IEnumerator StandUpAsync()
    {
        CheckConditionsToStandUp(out Vector3 newBasePosition, out Quaternion newBaseRotation, out _, out _, out float dot);
        yield return StandUpAsync(newBasePosition, newBaseRotation, dot);
    }
    public IEnumerator StandUpAsync(Vector3 newBasePosition, Quaternion newBaseRotation, float dot)
    {
        // Save current orientation of root bone
        Vector3 rootBonePosition = rootBone.position;
        Quaternion rootBoneRotation = rootBone.rotation;

        // Update base transform to be at the position where the ragdoll should be when standing up.
        baseTransform.SetPositionAndRotation(newBasePosition, newBaseRotation);
        //puppetmaster.Teleport(position, rotation, false);

        // Re-apply root bone orientation, to ensure it doesn't visually change
        rootBone.SetPositionAndRotation(rootBonePosition, rootBoneRotation);

        yield return null;

        // Initiate animation (and provide data so the animator knows which standing up animation needs to be used)
        animator.SetFloat(ragdollOrientationDotProduct, dot);
        animator.SetTrigger(standUpTrigger);

        // Re-enable animations, but don't immediately reset weights
        SetRagdollActive(true);
        // Slowly interpolate weights up to full power, over specific timeframes
        yield return MiscFunctions.WaitOnLerp(standUpTime, (ref float t) =>
        {
            float timeElapsed = t * standUpTime;

            // Have one AnimationCurve for puppetmaster weights, and one for IK weights
            float puppetMasterWeight = puppetmasterWeightCurve.Evaluate(timeElapsed);
            puppetmaster.muscleWeight = puppetMasterWeight;
            puppetmaster.pinWeight = puppetMasterWeight;
            float ikWeight = ikWeightCurve.Evaluate(timeElapsed);
            SetIKWeight(ikWeight);
        });

        // Re-enable AI functionality
        SetAIFunctionsActive(true);
    }
    /// <summary>
    /// Does everything in StandUpAsync(), but instantly.
    /// </summary>
    public void ForceAllStandUpValues()
    {
        SetRagdollActive(true);
        puppetmaster.muscleWeight = 1;
        puppetmaster.pinWeight = 1;
        SetIKWeight(1);
        SetAIFunctionsActive(true);
    }

    public void CheckConditionsToStandUp(out Vector3 newBasePosition, out Quaternion newBaseRotation, out bool navMeshPointFound, out NavMeshHit navMeshHit, out float dot)
    {
        // Check conditions for ragdoll to stand up again
        Bounds ragdollBounds = TransformUtility.CombinedBounds(colliders);
        PhysicsAffectedAI.CalculateBasePositionBasedOnRagdoll(baseTransform, rootBone, ragdollBounds, out newBasePosition, out newBaseRotation, out dot);
        navMeshPointFound = NavMesh.SamplePosition(newBasePosition, out navMeshHit, navMeshCheckDistance, NavMesh.AllAreas);
        if (navMeshPointFound) newBasePosition = navMeshHit.position;
    }

    void SetRagdollActive(bool alive)
    {
        puppetmaster.state = alive ? PuppetMaster.State.Alive : PuppetMaster.State.Dead;

        
        if (!alive)
        {
            Vector3 velocity = centralRigidbody.velocity;
            Vector3 angularVelocity = centralRigidbody.angularVelocity;
            Vector3 accumulatedForce = centralRigidbody.GetAccumulatedForce();
            Vector3 accumulatedTorque = centralRigidbody.GetAccumulatedTorque();
            Debug.Log($"Starting ragdoll, transferring velocity from main rigidbody ({velocity}, {angularVelocity}) to children");
            foreach (Muscle muscle in puppetmaster.muscles)
            {
                muscle.rigidbody.AddForce(velocity, ForceMode.VelocityChange);
                //muscle.rigidbody.AddForce(accumulatedForce, ForceMode.Force);
                muscle.rigidbody.AddTorque(angularVelocity, ForceMode.VelocityChange);
                //muscle.rigidbody.AddTorque(accumulatedTorque, ForceMode.Force);
            }
        }

        centralCollider.enabled = alive;
        centralRigidbody.isKinematic = !alive;
    }
    void SetAIFunctionsActive(bool active)
    {
        if (rootAI == null) return;
        rootAI.visionCone.enabled = active;
        rootAI.hearing.enabled = active;
        rootAI.aiming.enabled = active;
        rootAI.agent.enabled = active;
    }
    void SetIKWeight(float weight)
    {
        foreach (IK ik in ikComponents)
        {
            //ik.enabled = weight > 0;
            ik.GetIKSolver().SetIKPositionWeight(weight);
        }
    }
}