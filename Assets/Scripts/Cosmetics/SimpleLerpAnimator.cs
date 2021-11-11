using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLerpAnimator : MonoBehaviour
{
    [System.Serializable]
    public struct SimpleLerpAnimation
    {
        public string name;
        public Transform older;
        public Transform newer;
        public float time;
        public AnimationCurve curve;

        public static SimpleLerpAnimation New
        {
            get
            {
                return new SimpleLerpAnimation
                {
                    older = null,
                    newer = null,
                    name = "New Animation",
                    time = 0.25f,
                    curve = AnimationCurve.EaseInOut(0, 0, 1, 1),
                };
            }
        }
    }
    
    public Transform transformToAnimate;
    public SimpleLerpAnimation[] animations = new SimpleLerpAnimation[] { SimpleLerpAnimation.New };

    Transform oldModelOrientation;
    Transform newModelOrientation;
    float animationTime;
    AnimationCurve animationCurve;
    float animationTimer;

    private void LateUpdate()
    {
        if (oldModelOrientation != null && newModelOrientation != null)
        {
            animationTimer += Time.deltaTime / animationTime;
            float lerpValue = animationCurve.Evaluate(animationTimer);
            transformToAnimate.position = Vector3.Lerp(oldModelOrientation.position, newModelOrientation.position, lerpValue);
            transformToAnimate.rotation = Quaternion.Lerp(oldModelOrientation.rotation, newModelOrientation.rotation, lerpValue);
        }
    }
    public void PlayAnimation(SimpleLerpAnimation animation)
    {
        oldModelOrientation = animation.older;
        newModelOrientation = animation.newer;
        animationTime = animation.time;
        animationCurve = animation.curve;
        animationTimer = 0;
    }
    public void PlayAnimation(int index)
    {
        if (Mathf.Clamp(index, 0, animations.Length - 1) != index)
        {
            Debug.LogError("An animation does not exist at this index!");
            return;
        }

        PlayAnimation(animations[index]);
    }
}
