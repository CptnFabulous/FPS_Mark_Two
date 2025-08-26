using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CptnFabulous.ObjectPool;

public class RangedAttackSmokeEmitter : MonoBehaviour
{
    public ContinuousRangedAttackData attachedTo;
    public SmokeCloud smokeCloudPrefab;

    SmokeCloud assignedSmokeCloud;
    
    private void OnEnable() => RequestParticleSystem();
    private void OnDisable() => DismissParticleSystem();

    void RequestParticleSystem()
    {
        if (assignedSmokeCloud != null) return;

        // Create an object pool and request a particle system from it.
        // Determine settings beforehand, to make sure the particle system isn't disabled after being dismissed.
        // This ensures the spawned particles continue to exist in the world even when the weapon is holstered.
        ObjectPool.CreateObjectPool(smokeCloudPrefab, true, 0, false);
        assignedSmokeCloud = ObjectPool.RequestObject(smokeCloudPrefab);

        // Make it a child of the muzzle
        Transform psTransform = assignedSmokeCloud.transform;
        psTransform.parent = attachedTo.muzzle;
        psTransform.localPosition = Vector3.zero;
        psTransform.localRotation = Quaternion.identity;
        psTransform.localScale = Vector3.one;

        assignedSmokeCloud.emitting = true;
    }
    void DismissParticleSystem()
    {
        if (assignedSmokeCloud == null) return;

        // Turn off particle system and dismiss it to the object pool.
        assignedSmokeCloud.emitting = false;
        ObjectPool.DismissObject(assignedSmokeCloud);
        // Clear reference
        assignedSmokeCloud = null;
    }
}
