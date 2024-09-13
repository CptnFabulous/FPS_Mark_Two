using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerRagdollJitterOnDamageType : MonoBehaviour
{
    [SerializeField] RagdollJittering jitterHandler;
    [SerializeField] DamageType typeToTrigger = DamageType.Electrocution;

    Ragdoll baseRagdoll => jitterHandler.baseRagdoll;

    void Awake()
    {
        //baseRagdoll.onActiveStateSet.AddListener(OnRagdollActiveStateSet);
        baseRagdoll.attachedTo.health.onDamage.AddListener(OnDamage);
    }
    /*
    void OnRagdollActiveStateSet(bool active)
    {
        if (active == false) return;

        DamageMessage damage = baseRagdoll.attachedTo.health.lastSourceOfDamage;
        if (damage == null || damage.method != typeToTrigger) return;

        // Start jitter
        jitterHandler.StartJitter();
    }
    */
    void OnDamage(DamageMessage dm)
    {
        // Check if ragdoll is active
        if (baseRagdoll.enabled == false) return;
        // Check if damage type is correct
        if (dm.method != typeToTrigger) return;
        // Start jitter
        jitterHandler.StartJitter();
    }

}