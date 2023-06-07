using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilController : MonoBehaviour
{
    public LookController lookControls;
    [SerializeField] float recoilRecoveryDegreesPerSecond = 10;
    [SerializeField] float timeAfterRecoilBeforeRecovery = 0.2f;
    [SerializeField] float aimSpeedToCancelRecoilRecovery = 15;

    Vector2 recoil;
    float lastTimeRecoiled;

    public Vector2 recoilValue
    {
        get => recoil;
        set
        {
            recoil = value;
            lastTimeRecoiled = Time.time;
        }
    }
    
    private void Update()
    {
        // If the player is putting out a strong enough input to be deliberately adjusting their aim from their current direction, don't recover the recoil.
        if (lookControls.processedAimInput.magnitude > aimSpeedToCancelRecoilRecovery)
        {
            lookControls.lookAngles += recoil;
            recoil = Vector2.zero;
        }
        else if (Time.time - lastTimeRecoiled > timeAfterRecoilBeforeRecovery)
        {
            recoil = Vector2.MoveTowards(recoil, Vector2.zero, recoilRecoveryDegreesPerSecond * Time.deltaTime);
        }
    }

    public IEnumerator AddRecoilOverTime(Vector2 degrees, float time, AnimationCurve curve)
    {
        float timer = 0;
        float curveLastFrame = 0;

        while (timer != 1)
        {
            timer += Time.deltaTime / time;
            timer = Mathf.Clamp01(timer);

            float curveThisFrame = curve.Evaluate(timer);
            float curveDeltaTime = curveThisFrame - curveLastFrame;

            Vector2 toAddThisFrame = degrees * curveDeltaTime;
            recoilValue += toAddThisFrame;

            yield return new WaitForEndOfFrame();
            curveLastFrame = curveThisFrame;
        }
    }
}