using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilController : MonoBehaviour
{
    public Transform recoillingTransform;
    public LookController lookControls;
    [SerializeField] float recoilRecoveryTime = 1;
    [SerializeField] float timeAfterRecoilBeforeRecovery = 0.2f;
    [SerializeField] AnimationCurve recoilRecoveryCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // How the recoil drops over time, to ensure it's smooth
    [SerializeField] float aimSpeedToCancelRecoilRecovery = 15; // If the player's aim input is stronger than this, cancel the recoil recovery

    Vector2 recoil; // The accumulated recoil value
    float lastTimeRecoiled; // The last time recoil force was applied
    Vector2 previousRecoil; // What the recoil was before recovery

    //public Vector2 recoilValue => recoil;
    
    private void Update()
    {
        if (recoil.sqrMagnitude <= 0) return;

        // If the player is putting out a strong enough input to be deliberately adjusting their aim from their current direction, don't recover the recoil.
        if (lookControls.GetProcessedAimInput().magnitude > aimSpeedToCancelRecoilRecovery)
        {
            CancelRecoil();
        }

        // Check again if there's still recoil to recover from. If so, shift the value back towards zero.
        if (recoil.sqrMagnitude > 0)
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

        recoillingTransform.localEulerAngles = new Vector3(-recoil.y, recoil.x, 0);
    }

    public void AddRecoil(Vector2 value)
    {
        recoil += value;
        lastTimeRecoiled = Time.time;
        previousRecoil = recoil;
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
            AddRecoil(toAddThisFrame);

            yield return new WaitForEndOfFrame();
            curveLastFrame = curveThisFrame;
        }
    }

    public void CancelRecoil()
    {
        // Calculate a Vector2 value to move the recoil value directly into lookAngles
        Vector2 toAdd = recoil;
        // (Don't adjust the vertical value if it would exceed the min or max degrees)
        float y = lookControls.lookAngles.y + toAdd.y;
        float yClamped = Mathf.Clamp(y, lookControls.minAngle, lookControls.maxAngle);
        if (yClamped != y) toAdd.y = 0;
        // Add it to lookControls.lookAngles, then subtract from recoil (and previousRecoil as well so if there's any value left the lerping is still consistent)
        lookControls.lookAngles += toAdd;
        recoil -= toAdd;
        previousRecoil -= toAdd;
    }
}