using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class VisibilityMeter : MonoBehaviour
{
    public Graphic display;
    public Gradient gradient;

    public UnityEvent<float> onBrightnessUpdated;

    //float minBrightness = 10;
    float maxBrightness = 20;

    public Player targetPlayer => p ??= GetComponentInParent<Player>();
    Player p;

    // Update is called once per frame
    void LateUpdate()
    {
        float value = DiegeticLightSource.EntityIllumination(targetPlayer);
        //Debug.Log(value);
        float t = Mathf.Clamp01(value / maxBrightness);
        display.color = gradient.Evaluate(t);
        onBrightnessUpdated.Invoke(t);
        
    }
}
