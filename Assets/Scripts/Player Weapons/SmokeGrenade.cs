using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeGrenade : Throwable
{
    public SmokeCloud smoke;
    public AudioSource audioSource;

    public override void Disable()
    {
        smoke.emitting = false;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.enabled = false;
        }
    }
    public override void OnThrow()
    {
        smoke.emitting = true;
        if (audioSource != null)
        {
            audioSource.enabled = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        ParticleSystem emitter = smoke.particleEmitter;
        if (audioSource != null)
        {
            float progress = emitter.time / emitter.main.duration;
            float emissionAtTime = emitter.emission.rateOverTime.Evaluate(progress);
            float maxEmission = emitter.emission.rateOverTimeMultiplier;
            float volume = emissionAtTime / maxEmission;
            //Debug.Log($"{progress}, {emissionAtTime}, {maxEmission}, {volume}");
            audioSource.volume = Mathf.Clamp01(volume);
        }
        
        if (emitter.isEmitting == false)
        {
            enabled = false;
            return;
        }
    }
}
