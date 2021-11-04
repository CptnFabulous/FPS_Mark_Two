using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [Header("Attack modes")]
    public WeaponMode[] modes;
    public int currentModeIndex;
    public WeaponMode CurrentMode
    {
        get
        {
            if (modes.Length < 1)
            {
                modes = new WeaponMode[1];
            }
            return modes[currentModeIndex];
        }
    }

    [Header("Switching")]
    public float switchSpeed;
    public UnityEvent onDraw;
    public UnityEvent onHolster;
    public bool isSwitching { get; private set; }

    [Header("Cosmetics")]
    public Renderer model;
    public Transform modelTransform;


    public bool InAction
    {
        get
        {
            if (isSwitching)
            {
                return true;
            }
            if (CurrentMode.InAction)
            {
                return true;
            }
            return false;
        }
    }

    public IEnumerator Draw()
    {
        //yield return new WaitUntil(() => isSwitching == false);
        isSwitching = true;
        gameObject.SetActive(true);
        onDraw.Invoke();

        yield return new WaitForSeconds(switchSpeed);

        isSwitching = false;
    }
    public IEnumerator Holster()
    {
        yield return new WaitUntil(() => InAction == false);

        isSwitching = true;
        onHolster.Invoke();

        yield return new WaitForSeconds(switchSpeed);

        gameObject.SetActive(false);
        isSwitching = false;
    }
    public IEnumerator SwitchMode(int newModeIndex)
    {
        if (InAction == true)
        {
            yield break;
        }

        isSwitching = true;
        modes[newModeIndex].onSwitch.Invoke();

        yield return new WaitForSeconds(modes[newModeIndex].switchSpeed);

        currentModeIndex = newModeIndex;
        isSwitching = false;
    }







    private void LateUpdate()
    {
        if (oldModelOrientation != null && newModelOrientation != null)
        {
            animationTimer += Time.deltaTime / animationTime;
            float lerpValue = animationCurve.Evaluate(animationTimer);
            modelTransform.position = Vector3.Lerp(oldModelOrientation.position, newModelOrientation.position, lerpValue);
            modelTransform.rotation = Quaternion.Lerp(oldModelOrientation.rotation, newModelOrientation.rotation, lerpValue);
        }
    }
    public void ApplyModelAnimation(SimpleWeaponAnimation animation)
    {
        oldModelOrientation = animation.older;
        newModelOrientation = animation.newer;
        animationTime = animation.time;
        animationCurve = animation.curve;
        animationTimer = 0;
    }
    
    Transform oldModelOrientation;
    Transform newModelOrientation;
    float animationTime;
    AnimationCurve animationCurve;
    float animationTimer;
}