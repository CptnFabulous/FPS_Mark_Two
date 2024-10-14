using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimedCosmeticEffect : MonoBehaviour
{
    [Range(0, 1)] public float timer;

    public float duration = 1;

    public bool looping;
    public bool playOnAwake;
    public bool stopOnAwake = true;
    public UnityEvent<float> effects;

    RectTransform rt;

    public bool completed => looping == false && timer >= 1;
    public RectTransform rectTransform => rt ??= GetComponent<RectTransform>();

    private void OnValidate()
    {
        effects.Invoke(timer);
    }
    /*
    private void Awake()
    {
        Stop();
    }
    */
    private void Start()
    {
        if (playOnAwake)
        {
            Play();
        }
        else if (stopOnAwake)
        {
            Stop();
        }
    }
    void LateUpdate()
    {
        // This seems unnecessary but is needed because even if Stop or Pause is run on the same frame, LateUpdate will still run for said frame.
        if (enabled == false) return;

        timer += Time.deltaTime / duration;
        timer = Mathf.Clamp01(timer);
        effects.Invoke(timer);
        if (timer == 1)
        {
            if (looping)
            {
                //timer %= 1;
                timer = 0;
            }
            else
            {
                enabled = false;
            }
        }
    }

    public void Play()
    {
        timer = 0;
        effects.Invoke(timer);
        enabled = true;
    }
    public void Pause()
    {
        effects.Invoke(timer);
        enabled = false;
    }
    public void Resume()
    {
        effects.Invoke(timer);
        enabled = true;
    }
    public void Stop()
    {
        enabled = false;
        timer = 0;
        effects.Invoke(timer);
    }
    public void SetTimeManually(float value)
    {
        timer = Mathf.Clamp01(value);
        effects.Invoke(timer);
    }
}