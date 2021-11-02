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

    [HideInInspector] public bool isSwitchingMode;
    WaitForSeconds switchYield;

    [Header("Cosmetics")]
    public Renderer model;
    public Transform modelTransform;




    public IEnumerator Draw(WeaponHandler handler)
    {
        if (handler.isSwitching == true)
        {
            yield break;
        }

        handler.isSwitching = true;
        gameObject.SetActive(true);
        onDraw.Invoke();
        handler.onDraw.Invoke(this);
        yield return switchYield;
        handler.isSwitching = false;
    }
    public IEnumerator Holster(WeaponHandler handler)
    {
        if (handler.isSwitching == true)
        {
            yield break;
        }

        handler.isSwitching = true;
        onHolster.Invoke();
        handler.onHolster.Invoke(this);
        yield return switchYield;
        gameObject.SetActive(false);
        handler.isSwitching = false;
    }
    public IEnumerator SwitchMode(int newModeIndex, WeaponHandler handler)
    {
        if (isSwitchingMode == true)
        {
            yield break;
        }

        isSwitchingMode = true;
        modes[newModeIndex].onSwitch.Invoke();
        yield return new WaitForSeconds(modes[newModeIndex].switchSpeed);
        currentModeIndex = newModeIndex;
        handler.onModeSwitch.Invoke(CurrentMode);
        isSwitchingMode = false;
    }



    private void Awake()
    {
        switchYield = new WaitForSeconds(switchSpeed);
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
    
    //SimpleWeaponAnimation currentAnimation;
    float animationTimer;

    /*
    private void LateUpdate()
    {
        if (modelOrientation != null)
        {
            modelTransform.position = Vector3.MoveTowards(transform.position, modelOrientation.position, animateMoveSpeed * Time.deltaTime);
            modelTransform.rotation = Quaternion.RotateTowards(transform.rotation, modelOrientation.rotation, animateRotateSpeed * Time.deltaTime);
        }
        
    }
    public void AnimateModelOrientation(SimpleWeaponAnimation animation)
    {
        // Assign new position so model moves to it
        // If old position is assigned, teleport to old position first

        if (animation.oldOrientation != null)
        {
            modelTransform.position = animation.oldOrientation.position;
            modelTransform.rotation = animation.oldOrientation.rotation;
        }
        modelOrientation = animation.newOrientation;
        animateMoveSpeed = Vector3.Distance(modelTransform.position, modelOrientation.position) / animation.time;
        animateRotateSpeed = Quaternion.Angle(modelTransform.rotation, modelOrientation.rotation) / animation.time;
    }
    Transform modelOrientation;
    float animateMoveSpeed;
    float animateRotateSpeed;
    */
}