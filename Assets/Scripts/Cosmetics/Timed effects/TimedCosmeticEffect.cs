using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimedCosmeticEffect : MonoBehaviour
{
    public float duration = 1;
    
    public bool looping;
    public UnityEvent<float> effects;

    float timer;

    private void OnValidate()
    {
        effects.Invoke(timer);
    }
    private void Start()
    {
        Stop();
        effects.Invoke(timer);
    }
    void LateUpdate()
    {
        timer += Time.deltaTime / duration;
        timer = Mathf.Clamp01(timer);
        effects.Invoke(timer);
        if (timer == 1)
        {
            if (looping)
            {
                timer = 0;
            }
            else
            {
                Stop();
            }
        }
    }

    public void Play()
    {
        Debug.Log("Playing");
        timer = 0;
        enabled = true;
    }
    public void Pause()
    {
        enabled = false;
    }
    public void Resume()
    {
        enabled = true;
    }
    public void Stop()
    {
        timer = 0;
        enabled = false;
    }
    public void SetTimeManually(float value)
    {
        timer = Mathf.Clamp01(value);
        effects.Invoke(timer);
    }
}
