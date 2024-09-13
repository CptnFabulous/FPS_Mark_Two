using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollJittering : MonoBehaviour
{
    public Ragdoll baseRagdoll;
    public float defaultJitterDuration = 5;
    public float jitterFrequency = 0.1f;
    public float jitterDegrees = 50;

    float lastTimeJittered = Mathf.NegativeInfinity;

    [HideInInspector, System.NonSerialized] public float remainingJitterTime = 0;

    private void Awake()
    {
        //baseRagdoll.onActiveStateSet.AddListener((active) => enabled = active);
        baseRagdoll.onActiveStateSet.AddListener(OnRagdollActiveStateSet);
        /*
        baseRagdoll.attachedTo.health.onDamage.AddListener((dm) =>
        {
            if (dm.method == DamageType.Electrocution) StartJitter();
        });
        */

        // Modify max angular velocity to allow for jittering
        if (jitterDegrees > 0 && Physics.defaultMaxAngularSpeed < jitterDegrees)
        {
            foreach (Rigidbody rb in baseRagdoll.rigidbodies)
            {
                rb.maxAngularVelocity = Mathf.Max(rb.maxAngularVelocity, jitterDegrees);
            }
        }
    }
    private void FixedUpdate()
    {
        if (baseRagdoll.enabled == false) return;

        // Check if the ragdoll should still be jittering
        if (remainingJitterTime <= 0) return;
        remainingJitterTime -= Time.fixedDeltaTime;

        if (Time.time - lastTimeJittered >= jitterFrequency)
        {
            // Apply a random rigidbody force to each joint
            foreach (Rigidbody rb in baseRagdoll.rigidbodies)
            {
                Vector3 eulerAngles = Random.onUnitSphere * jitterDegrees;
                rb.AddTorque(eulerAngles, ForceMode.VelocityChange);
            }
            // Reset timer
            lastTimeJittered = Time.time;
        }
    }

    public void StartJitter() => StartJitter(defaultJitterDuration);
    public void StartJitter(float seconds)
    {
        if (enabled == false || baseRagdoll.enabled == false) return;
        Debug.Log($"Setting {seconds} seconds of jitter");
        remainingJitterTime = seconds;
    }




    void OnRagdollActiveStateSet(bool active)
    {
        enabled = active;
        /*
        if (active == false) return;

        Health h = baseRagdoll.attachedTo.health;
        DamageMessage damage = h.lastSourceOfDamage;
        if (damage == null || damage.method != DamageType.Electrocution) return;

        StartJitter();
        */
    }
}