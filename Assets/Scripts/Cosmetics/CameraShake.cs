using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Effect stats")]
    public float duration = 0.25f;
    public float shakeSpeed = 50f;
    public float intensity = 0.05f;
    public AnimationCurve durationCurve = new AnimationCurve
    {
        keys = new Keyframe[]
        {
            new Keyframe(0, 0),
            new Keyframe(0.1f, 1),
            new Keyframe(1, 0),
        }
    };

    [Header("Effect range")]
    public float range = 10f;
    public AnimationCurve falloffCurve = AnimationCurve.Linear(0, 1, 1, 0);

    public void Play()
    {
        MovementController[] playersInScene = FindObjectsOfType<MovementController>(false);
        for (int i = 0; i < playersInScene.Length; i++)
        {
            float distance = Vector3.Distance(playersInScene[i].transform.position, transform.position);
            if (distance < range)
            {
                playersInScene[i].StartCoroutine(ShakeSequence(playersInScene[i], distance / range));
            }
        }
    }


    public void Play(MovementController player, float intensity)
    {
        player.StartCoroutine(ShakeSequence(player, intensity));
    }

    IEnumerator ShakeSequence(MovementController player, float intensityMultiplier)
    {
        float timer = 0;
        while (timer != 1)
        {
            // Wait until after regular Update() but before LateUpdate() is processed, since WaitForEndOfFrame occurs after rendering has occurred
            yield return null;
            timer += Time.deltaTime / duration;
            timer = Mathf.Clamp01(timer);

            // Generates changing values from noise
            float noiseTime = Time.time * shakeSpeed;
            Vector2 noise = new Vector2(Mathf.PerlinNoise(noiseTime, 0), Mathf.PerlinNoise(0, noiseTime));
            noise -= new Vector2(0.5f, 0.5f);
            noise *= 2;
            Vector2 deviations = intensity * intensityMultiplier * durationCurve.Evaluate(timer) * noise;

            player.worldViewCamera.transform.localRotation = Quaternion.Euler(deviations.x, deviations.y, 0);
        }
        player.worldViewCamera.transform.localRotation = Quaternion.identity;
    }

    /*
    public void OnParticleTrigger()
    {
        
    }
    */
}
