using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilController : MonoBehaviour
{
    public LookController lookControls;
    [SerializeField] float recoilRecoveryTime = 1;
    [SerializeField] float timeAfterRecoilBeforeRecovery = 0.2f;
    [SerializeField] AnimationCurve recoilRecoveryCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // How the recoil drops over time, to ensure it's smooth
    [SerializeField] float aimSpeedToCancelRecoilRecovery = 15; // If the player's aim input is stronger than this, cancel the recoil recovery

    Vector2 recoil; // The accumulated recoil value
    float lastTimeRecoiled; // The last time recoil force was applied
    Vector2 previousRecoil; // What the recoil was before recovery

    public Vector2 recoilValue
    {
        get => recoil;
        set
        {
            recoil = value;
            lastTimeRecoiled = Time.time;
            previousRecoil = recoil;
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

        // Check again if there's still recoil to recover from. If so, shift the value back towards zero.
        if (recoil.magnitude > 0)
        {
            // Calculate how long it's been since the recoil was last influenced
            float time = Time.time - lastTimeRecoiled - timeAfterRecoilBeforeRecovery;
            if (time > 0)
            {
                // If the time was greater than the delay before recovery, lerp back to zero based on how much time has passed
                float t = recoilRecoveryCurve.Evaluate(time / recoilRecoveryTime);
                t = Mathf.Clamp01(t);
                recoil = Vector2.Lerp(Vector2.zero, previousRecoil, t);
            }
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