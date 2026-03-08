using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ResourceDisplay : MonoBehaviour
{
    [Header("Text display")]
    [SerializeField] Text amount;
    public string decimalFormatting = "0";

    [Header("Additional animations")]
    [SerializeField] Animator animationController;
    [SerializeField] string current = "Current";
    [SerializeField] string critical = "Critical";
    [SerializeField] string full = "Full";
    [SerializeField] string depleted = "Depleted";

    public System.Func<Resource> obtainValues;
    Resource lastObtainedValues;

    protected virtual void LateUpdate()
    {
        if (obtainValues == null) return;
        Resource currentValues = obtainValues.Invoke();
        if (currentValues != lastObtainedValues) Refresh(currentValues);
    }

    protected virtual void Refresh(Resource values)
    {
        // Set amount as text
        if (amount != null) amount.text = values.current.ToString(decimalFormatting);

        if (animationController != null)
        {
            TrySetAnimatorFloat(animationController, current, values.current);
            TrySetAnimatorBool(animationController, critical, values.isCritical);
            TrySetAnimatorBool(animationController, full, values.isFull);
            TrySetAnimatorBool(animationController, depleted, values.isDepleted);
        }
    }

    public static bool TrySetAnimatorTrigger(Animator animator, string name)
    {
        if (!AnimatorParameterExists(animator, name)) return false;
        animator.SetTrigger(name);
        return true;
    }
    public static bool TrySetAnimatorBool(Animator animator, string name, bool value)
    {
        if (!AnimatorParameterExists(animator, name)) return false;
        animator.SetBool(name, value);
        return true;
    }
    public static bool TrySetAnimatorInteger(Animator animator, string name, int value)
    {
        if (!AnimatorParameterExists(animator, name)) return false;
        animator.SetInteger(name, value);
        return true;
    }
    public static bool TrySetAnimatorFloat(Animator animator, string name, float value)
    {
        if (!AnimatorParameterExists(animator, name)) return false;
        animator.SetFloat(name, value);
        return true;
    }
    public static bool AnimatorParameterExists(Animator animator, string name)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == name) return true;
        }
        return false;
    }
}